﻿using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace HotPotato.E2E.Test
{
    [Collection("Host")]
    public class SanityTest
    {
        private IWebHost host;

        private const string ApiLocation = "http://localhost:9191";
        private const string Endpoint = "/endpoint";
        private const string ProxyEndpoint = "http://localhost:3232/endpoint";
        private const string GetMethodCall = "GET";
        private const string OKResponseMessage = "OK";
        private const string NotFoundResponseMessage = "Not Found";
        private const string InternalServerErrorResponseMessage = "Internal Server Error";
        private const string PlainTextContentType = "text/plain";
        private const string ApplicationJsonContentType = "application/json";
        private const string ContentType = "Content-Type";

        public SanityTest(HostFixture fixture)
        {
            host = fixture.host;
        }

        [Fact]
        public async Task HotPotato_Should_Return_OK_And_A_String()
        {
            //Setting up mock server to hit
            const string expected = "ValidResponse";

            using (var server = FluentMockServer.Start(ApiLocation))
            {
                server
                    .Given(
                        Request.Create()
                            .WithPath(Endpoint)
                            .UsingGet()
                    )
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200)
                            .WithHeader(ContentType, PlainTextContentType)
                            .WithBody(expected)
                    );

                using (HttpClient client = new HttpClient())
                {
                    HttpMethod method = new HttpMethod(GetMethodCall);

                    using (HttpRequestMessage req = new HttpRequestMessage(method, ProxyEndpoint))
                    {
                        HttpResponseMessage res = await client.SendAsync(req);

                        //Asserts
                        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
                        Assert.Equal(OKResponseMessage, res.ReasonPhrase);
                        Assert.Equal(13, res.Content.Headers.ContentLength);
                        Assert.Equal(PlainTextContentType, res.Content.Headers.ContentType.MediaType);
                        Assert.Equal(expected, res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }

        [Fact]
        public async Task HotPotato_Should_Return_OK_And_A_JSON_Object()
        {
            //Setting up mock server to hit
            string json = @"{
                'Email': 'james@example.com',
                'Active': true,
                'CreatedDate': '2013-01-20T00:00:00Z',
                'Roles': [
                    'User',
                    'Admin'
                ]}";

            using (var server = FluentMockServer.Start(ApiLocation))
            {
                server
                    .Given(
                        Request.Create()
                            .WithPath(Endpoint)
                            .UsingGet()
                    )
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(200)
                            .WithHeader(ContentType, ApplicationJsonContentType)
                            .WithBody(json)
                    );

                using (HttpClient client = new HttpClient())
                {
                    HttpMethod method = new HttpMethod(GetMethodCall);

                    using (HttpRequestMessage req = new HttpRequestMessage(method, ProxyEndpoint))
                    {

                        HttpResponseMessage res = await client.SendAsync(req);

                        //Asserts
                        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
                        Assert.Equal(OKResponseMessage, res.ReasonPhrase);
                        Assert.Equal(ApplicationJsonContentType, res.Content.Headers.ContentType.MediaType);
                        Assert.Equal(json, res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }

        [Fact]
        public async Task HotPotato_Should_Return_404_Error()
        {
            using (var server = FluentMockServer.Start(ApiLocation))
            {
                server
                    .Given(
                        Request.Create()
                            .WithPath(Endpoint)
                            .UsingGet()
                    )
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(404)
                            .WithBody(NotFoundResponseMessage)
                    );


                using (HttpClient client = new HttpClient())
                {
                    HttpMethod method = new HttpMethod(GetMethodCall);

                    using (HttpRequestMessage req = new HttpRequestMessage(method, ProxyEndpoint))
                    {

                        HttpResponseMessage res = await client.SendAsync(req);

                        //Asserts
                        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
                        Assert.Equal(NotFoundResponseMessage, res.ReasonPhrase);
                        Assert.Equal(NotFoundResponseMessage, res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }

        [Fact]
        public async Task HotPotato_Should_Return_500_Error()
        {
            using (var server = FluentMockServer.Start(ApiLocation))
            {
                server
                    .Given(
                        Request.Create()
                            .WithPath(Endpoint)
                            .UsingGet()
                    )
                    .RespondWith(
                        Response.Create()
                            .WithStatusCode(500)
                            .WithBody(InternalServerErrorResponseMessage)
                    );


                //Setting up Http Client
                using (HttpClient client = new HttpClient())
                {
                    HttpMethod method = new HttpMethod(GetMethodCall);

                    using (HttpRequestMessage req = new HttpRequestMessage(method, ProxyEndpoint))
                    {

                        HttpResponseMessage res = await client.SendAsync(req);

                        //Asserts
                        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
                        Assert.Equal(InternalServerErrorResponseMessage, res.ReasonPhrase);
                        Assert.Equal(InternalServerErrorResponseMessage, res.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }
    }
}
