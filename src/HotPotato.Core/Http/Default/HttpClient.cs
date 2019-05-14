﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HotPotato.Core.Http.Default
{
    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient client;
        //public HttpClient(System.Net.Http.HttpClient client)
        //{
        //    _ = client ?? throw new ArgumentNullException(nameof(client));
        //    this.client = client;

        //}
        public HttpClient(System.Net.Http.HttpClient client, IWebProxy proxy = null, ForwardProxy.HttpForwardProxyConfig proxyConfig = null)
        {
            _ = client ?? throw new ArgumentNullException(nameof(client));
            if (proxyConfig.enabled)
            { 
                System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler
                {
                    Proxy = proxy
                };
                this.client = new System.Net.Http.HttpClient(handler);
            }
            else
            {
                this.client = new System.Net.Http.HttpClient();
            }
        }

        public async Task<IHttpResponse> SendAsync(IHttpRequest request)
        {
            using (HttpResponseMessage response = await client.SendAsync(request.ToClientRequestMessage()))
            {
                return await response.ToClientResponseAsync();
            }
        }
    }
}
