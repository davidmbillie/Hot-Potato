using HotPotato.Core.Http.Default;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MSHTTP = Microsoft.AspNetCore.Http;

namespace HotPotato.Core.Http
{
	public static class HttpExtensions
	{
		private const string customHeaderPrefix = "X-HP-";
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

		/// <summary>
		/// Convert from hot potato's request to a request message to be sent to the client
		/// </summary>
		/// <param name="this"></param>
		/// <returns></returns>
		public static HttpRequestMessage ToClientRequestMessage(this IHotPotatoRequest @this)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));

			HttpRequestMessage message = new HttpRequestMessage(@this.Method, @this.Uri);
			if (@this.Content != null)
			{
				message.Content = @this.Content;
			}
			foreach (var item in @this.HttpHeaders)
			{
				if (!ExcludedRequestHeaders.Contains(item.Key.ToLowerInvariant()) && !message.Headers.TryAddWithoutValidation(item.Key, item.Value) && message.Content != null)
				{
					message.Content.Headers.TryAddWithoutValidation(item.Key, item.Value);
				}
			}
			return message;
		}

		/// <summary>
		/// Convert from an mshtml response message to a hot potato request task to be resolved by the client
		/// </summary>
		/// <param name="this"></param>
		/// <returns></returns>
		public static async Task<IHotPotatoResponse> ToClientResponseAsync(this HttpResponseMessage @this)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));

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

				contentType = contentType ?? new MediaTypeHeaderValue("application/json");
				byte[] payload = await @this.Content.ReadAsByteArrayAsync();

				return new HotPotatoResponse(@this.StatusCode, headers, payload, contentType);
			}
			else
			{
				return new HotPotatoResponse(@this.StatusCode, headers, Array.Empty<byte>(), new MediaTypeHeaderValue("application/json"));
			}
		}

		/// <summary>
		/// Converts the mshttp request to a hot potato request to be used by the proxy
		/// </summary>
		/// <param name="this"></param>
		/// <param name="remoteEndpoint"></param>
		/// <returns></returns>
		public static async Task<IHotPotatoRequest> ToProxyRequest(this MSHTTP.HttpRequest @this, string remoteEndpoint)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));
			_ = remoteEndpoint ?? throw Exceptions.ArgumentNull(nameof(remoteEndpoint));

			HotPotatoRequest request = new HotPotatoRequest(new HttpMethod(@this.Method), @this.BuildUri(remoteEndpoint));
			if (@this.Headers != null && @this.Headers.Count > 0)
			{
				foreach (var item in @this.Headers)
				{
					if (!item.Key.Contains(customHeaderPrefix))
					{
						request.HttpHeaders.Add(item.Key, item.Value.ToArray());
					}
					else
					{
						request.CustomHeaders.Add(item.Key, item.Value.ToArray());
					}
				}
			}
			if (MethodsWithPayload.Contains(@this.Method.ToUpperInvariant()) && @this.Body != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					await @this.Body.CopyToAsync(stream);
					if (!string.IsNullOrEmpty(@this.ContentType))
					{
						//Sanitize here since System.Net.Http was throwing a format exception
						//when sending POST/PUT requests with payloads through TestServer
						@this.ContentType = @this.ContentType.Split(';')[0];
					}
					request.SetContent(stream.ToArray(), @this.ContentType);
				}
			}

			return request;
		}

		/// <summary>
		/// Allow responses to be asynchronously processed by hot potato
		/// </summary>
		/// <param name="this"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static async Task ToProxyResponseAsync(this IHotPotatoResponse @this, MSHTTP.HttpResponse response)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));
			_ = response ?? throw Exceptions.ArgumentNull(nameof(response));

			response.StatusCode = (int)@this.StatusCode;

			if (@this.Headers != null)
			{
				foreach (var header in @this.Headers)
				{
					if (!ExcludedResponseHeaders.Contains(header.Key.ToLowerInvariant()))
					{
						response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
					}
				}
			}

			if (@this.Content.Length > 0)
			{
				await response.Body.WriteAsync(@this.Content, 0, @this.Content.Length);
			}
		}

		public static Uri BuildUri(this MSHTTP.HttpRequest @this, string remoteEndpoint)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));
			_ = remoteEndpoint ?? throw Exceptions.ArgumentNull(nameof(remoteEndpoint));

			return new Uri($"{remoteEndpoint}{@this.Path.Value}{@this.QueryString}");
		}

		public static string ToBodyString(this IHotPotatoResponse @this)
		{
			if (@this.Content == null || @this.ContentType == null)
			{
				return string.Empty;
			}

			byte[] bodyContent = @this.Content;

			Encoding encode = null;
			switch (@this.ContentType.CharSet)
			{
				case "utf-8":
					encode = Encoding.UTF8;
					break;
				case "utf-7":
#pragma warning disable
					encode = Encoding.UTF7;
#pragma warning restore
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
			return encode.GetString(bodyContent);
		}

		public static async Task<IHotPotatoRequest> ToHotPotatoRequest(this HttpRequest @this)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));

			HotPotatoRequest request = new HotPotatoRequest(new HttpMethod(@this.Method), new Uri(@this.GetDisplayUrl()));
			if (@this.Headers != null && @this.Headers.Count > 0)
			{
				foreach (var item in @this.Headers)
				{
					if (!item.Key.Contains(customHeaderPrefix))
					{
						request.HttpHeaders.Add(item.Key, item.Value.ToArray());
					}
					else
					{
						request.CustomHeaders.Add(item.Key, item.Value.ToArray());
					}
				}
			}
			if (MethodsWithPayload.Contains(@this.Method.ToUpperInvariant()) && @this.Body != null)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					await @this.Body.CopyToAsync(stream);
					if (!string.IsNullOrEmpty(@this.ContentType))
					{
						//Sanitize here since System.Net.Http was throwing a format exception
						//when sending POST/PUT requests with payloads through TestServer
						@this.ContentType = @this.ContentType.Split(';')[0];
					}
					request.SetContent(stream.ToArray(), @this.ContentType);
					// If we've read the body stream, seek back to the beginning
					if(@this.Body.CanSeek)
					{
						@this.Body.Seek(0, SeekOrigin.Begin);
					}
				}
			}

			return request;
		}

		public static async Task<IHotPotatoResponse> ToHotPotatoResponse(this HttpResponse @this)
		{
			_ = @this ?? throw Exceptions.ArgumentNull(nameof(@this));

			HttpHeaders headers = new HttpHeaders();
			if (@this.Headers != null)
			{
				foreach (var item in @this?.Headers)
				{
					headers.Add(item.Key.ToString(), item.Value.ToString());
				}
			}

			if (@this.Body != null)
			{
				string contentType = @this.ContentType;

				byte[] payload;
				using(MemoryStream content = new MemoryStream())
				{
					await @this.Body.CopyToAsync(content);
					payload = content.ToArray();
				}

				return new HotPotatoResponse((HttpStatusCode)@this.StatusCode, headers, payload, contentType);
			}
			else
			{
				return new HotPotatoResponse((HttpStatusCode)@this.StatusCode, headers, Array.Empty<byte>(), new MediaTypeHeaderValue("application/json"));
			}
		}
	}
}
