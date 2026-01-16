using HotPotato.Core.Cookies;
using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using HotPotato.OpenApi.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace HotPotato.E2E.Test
{
	[Collection("Host")]
	public class CookiesTest
	{
		private IHost host;
		private bool specTokenExists;

		private const string ApiLocation = "http://localhost:5000";
		private const string Endpoint = "/endpoint";
		private const string ProxyEndpoint = "http://localhost:3232/endpoint";
		private const string CookiesEndpoint = "http://localhost:3232/cookies";
		private const string GetMethodCall = "GET";
		private const string PlainTextContentType = "text/plain";
		private const string ContentType = "Content-Type";

		private const string CookieName = "TestCookie";
		private const string CookieValue = "TestValue";
		private const string CookiePath = "/";
		private const string CookieDomain = "localhost";

		public CookiesTest(HostFixture fixture)
		{
			host = fixture.Host;
			specTokenExists = fixture.SpecTokenExists;
		}

		[SkippableFact]
		public async Task HotPotato_CookiesEndpoint_Should_Delete_Cookies()
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

				ICookieJar cookieJar = servicePro.GetService<ICookieJar>();
				Cookie cookie = new Cookie(CookieName, CookieValue, CookiePath, CookieDomain);
				cookieJar.Cookies.Add(cookie);

				using (HotPotatoRequest req = new HotPotatoRequest(HttpMethod.Delete, new System.Uri(CookiesEndpoint)))
				{
					IHotPotatoResponse res = await client.SendAsync(req);
				}

				HttpMethod method = new HttpMethod(GetMethodCall);

				IResultCollector resultCollector = servicePro.GetService<IResultCollector>();
				resultCollector.Results.Clear();

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					IHotPotatoResponse res = await client.SendAsync(req);
				}

				Assert.True(cookieJar.Cookies.Count == 0);
			}
		}
	}
}
