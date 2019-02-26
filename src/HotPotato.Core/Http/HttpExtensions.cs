﻿using HotPotato.Core.Http.Default;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MSHTTP = Microsoft.AspNetCore.Http;

namespace HotPotato.Core.Http
{
    public static class HttpExtensions
    {
        private const string transferEncoding = "transfer-encoding";
        private static readonly HashSet<string> MethodsWithPayload = new HashSet<string>
        {
            "POST",
            "PATCH",
            "PUT"
        };

        private static readonly HashSet<string> ExcludedRequestHeaders =
            new HashSet<string>
            {
                "connection",
                "content-length",
                "keep-alive",
                "host",
                "upgrade",
                "upgrade-insecure-requests"
            };

        private static readonly HashSet<string> ExcludedResponseHeaders =
            new HashSet<string>
            {
                "connection",
                "server",
                "transfer-encoding",
                "upgrade",
                "x-powered-by"
            };

        public static HttpRequestMessage ToClientRequestMessage(this IHttpRequest @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));

            HttpRequestMessage message = new HttpRequestMessage(@this.Method, @this.Uri);
            if (@this.Content != null)
            {
                message.Content = @this.Content;
            }
            foreach (var item in @this.HttpHeaders)
            {
                if (!ExcludedRequestHeaders.Contains(item.Key))
                {
                    if (!message.Headers.TryAddWithoutValidation(item.Key, item.Value))
                    {
                        message.Content.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }
                }
            }
            return message;
        }
        public static async Task<IHttpResponse> ToClientResponseAsync(this HttpResponseMessage @this)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));

            HttpHeaders headers = new HttpHeaders();
            if (@this.Headers != null)
            {
                foreach (var item in @this?.Headers)
                {
                    headers.Add(item.Key, item.Value);
                }
            }
            if (@this.Content != null)
            {
                foreach (var item in @this.Content?.Headers)
                {
                    headers.Add(item.Key, item.Value);
                }
                MediaTypeHeaderValue contentType = @this.Content.Headers?.ContentType;
                byte[] payload = await @this.Content.ReadAsByteArrayAsync();
                return new HttpResponse(@this.StatusCode, headers, payload, contentType);
            }
            return new HttpResponse(@this.StatusCode, headers);
        }

        public static IHttpRequest ToProxyRequest(this MSHTTP.HttpRequest @this, string remoteEndpoint)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            _ = remoteEndpoint ?? throw new ArgumentNullException(nameof(remoteEndpoint));

            HttpRequest request = new HttpRequest(new HttpMethod(@this.Method), @this.BuildUri(remoteEndpoint));
            foreach (var item in @this?.Headers)
            {
                request.HttpHeaders.Add(item.Key, item.Value.ToArray());
            }
            if (MethodsWithPayload.Contains(@this.Method.ToUpperInvariant()) && @this.Body != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    @this.Body.CopyTo(stream);
                    request.SetContent(stream.ToArray(), @this.ContentType);
                }
            }

            return request;
        }

        public static async Task ToProxyResponseAsync(this IHttpResponse @this, MSHTTP.HttpResponse response)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            _ = response ?? throw new ArgumentNullException(nameof(response));

            response.StatusCode = (int)@this.StatusCode;

            response.Headers.Clear();
            if (@this.Headers != null)
            {
                foreach (var header in @this.Headers)
                {
                    if (!ExcludedResponseHeaders.Contains(header.Key))
                    {
                        response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                    }
                }
            }

            // HACK - Since calls are async, we don't need chunking.
            response.Headers.Remove(transferEncoding);

            if (@this.Content.Length > 0)
            {
                await response.Body.WriteAsync(@this.Content, 0, @this.Content.Length);
            }
        }

        public static Uri BuildUri(this MSHTTP.HttpRequest @this, string remoteEndpoint)
        {
            _ = @this ?? throw new ArgumentNullException(nameof(@this));
            _ = remoteEndpoint ?? throw new ArgumentNullException(nameof(remoteEndpoint));

            return new Uri($"{remoteEndpoint}{@this.Path.Value}{@this.QueryString}");
        }

        public static string ToBodyString(this IHttpResponse @this)
        {
            if (@this.Content == null || @this.ContentType == null)
            {
                return String.Empty;
            }
            else if (@this.ContentType.CharSet == null)
            {
                @this.ContentType.CharSet = String.Empty;
            }
            Encoding encode = null;
            switch (@this.ContentType.CharSet)
            {
                case "utf-8":
                    encode = Encoding.UTF8;
                    break;
                case "utf-7":
                    encode = Encoding.UTF7;
                    break;
                case "utf-32":
                    encode = Encoding.UTF32;
                    break;
                case "us-ascii":
                    encode = Encoding.ASCII;
                    break;
                default:
                    encode = Encoding.Default;
                    break;
            }
            return encode.GetString(@this.Content);
        }
    }
}
