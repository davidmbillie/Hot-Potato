using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
		private IHost host;
		private bool specTokenExists;

		private const string ApiLocation = "http://localhost:5000";
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
			host = fixture.Host;
			specTokenExists = fixture.SpecTokenExists;
		}

		[SkippableFact]
		public async Task HotPotato_Should_Return_OK_And_A_String()
		{
			Skip.IfNot(specTokenExists, TestConstants.MissingSpecToken);

			var servicePro = host.Services;

			//Setting up mock server to hit
			const string expected = "ValidResponse";

			using (var server = WireMockServer.Start(ApiLocation))
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

				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();

				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					IHotPotatoResponse res = await client.SendAsync(req);
                    
					//Asserts
					Assert.Equal(HttpStatusCode.OK, res.StatusCode);
					Assert.Equal(PlainTextContentType, res.ContentType.Type);
					Assert.Equal(expected, res.ToBodyString());
				}
			}
		}

		[SkippableFact]
		public async Task HotPotato_Should_Return_OK_And_A_JSON_Object()
		{
			Skip.IfNot(specTokenExists, TestConstants.MissingSpecToken);

			var servicePro = host.Services;

			//Setting up mock server to hit
			string json = @"{
				'Email': 'james@example.com',
				'Active': true,
				'CreatedDate': '2013-01-20T00:00:00Z',
				'Roles': [
					'User',
					'Admin'
				]}";

			using (var server = WireMockServer.Start(ApiLocation))
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

				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();

				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{

					IHotPotatoResponse res = await client.SendAsync(req);

					//Asserts
					Assert.Equal(HttpStatusCode.OK, res.StatusCode);
					Assert.Equal(ApplicationJsonContentType, res.ContentType.Type);
					Assert.Equal(json, res.ToBodyString());
				}
			}
		}

		[SkippableFact]
		public async Task HotPotato_Should_Return_404_Error()
		{
			Skip.IfNot(specTokenExists, TestConstants.MissingSpecToken);

			var servicePro = host.Services;

			using (var server = WireMockServer.Start(ApiLocation))
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


				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();
				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{

					IHotPotatoResponse res = await client.SendAsync(req);

					//Asserts
					Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
					Assert.Equal(NotFoundResponseMessage, res.ToBodyString());
				}
			}
		}

		[SkippableFact]
		public async Task HotPotato_Should_Return_500_Error()
		{
			Skip.IfNot(specTokenExists, TestConstants.MissingSpecToken);

			var servicePro = host.Services;

			using (var server = WireMockServer.Start(ApiLocation))
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
				HotPotatoClient client = (HotPotatoClient)servicePro.GetService<IHotPotatoClient>();
				HttpMethod method = new HttpMethod(GetMethodCall);

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{

					IHotPotatoResponse res = await client.SendAsync(req);

					//Asserts
					Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
					Assert.Equal(InternalServerErrorResponseMessage, res.ToBodyString());
				}
			}
		}
	}
}
