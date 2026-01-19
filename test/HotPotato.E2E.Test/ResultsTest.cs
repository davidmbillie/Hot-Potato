using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using HotPotato.OpenApi.Models;
using HotPotato.OpenApi.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace HotPotato.E2E.Test
{
	/// <summary>
	/// Since the OpenApi aspect of these tests is truncated by using mock endpoints,
	/// these examine the structure of the end Result objects rather than the validation process that creates them.
	/// Since the mock endpoint does not exist in the spec, the result collector/endpoint will always have a fail result for examination.
	/// </summary>
	[Collection("Host")]
	public class ResultsTest
	{
		private IHost host;
		private bool specTokenExists;

		private const string ApiLocation = "http://localhost:5000";
		private const string Endpoint = "/mockendpoint";
		private const string ProxyEndpoint = "http://localhost:3232/mockendpoint";
		private const string ResultsEndpoint = "http://localhost:3232/results";
		private const string GetMethodCall = "GET";
		private const string PlainTextContentType = "text/plain";
		private const string ContentType = "Content-Type";

		private const string ACustomHeaderKey = "X-HP-A-Custom-Header-Key";
		private const string ACustomHeaderValue = "A-Custom-Header-Value";

		private const string SerializedState = "Fail";

		public ResultsTest(HostFixture fixture)
		{
			host = fixture.Host;
			specTokenExists = fixture.SpecTokenExists;
		}


		// [SkippableFact]
		public async Task HotPotato_Should_Set_Respective_Custom_Headers()
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

				IResultCollector resultCollector = servicePro.GetService<IResultCollector>();
				resultCollector.Results.Clear();

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					req.HttpHeaders.Add(ACustomHeaderKey, ACustomHeaderValue);
					await client.SendAsync(req);
				}

				//second request to make sure custom header doesn't linger
				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					await client.SendAsync(req);
				}

				FailResultWithCustomHeaders resultWithCustomHeaders = (FailResultWithCustomHeaders)resultCollector.Results.ElementAt(0);
				Result resultWithoutCustomHeaders = resultCollector.Results.ElementAt(1);

				Assert.True(resultWithCustomHeaders.Custom.ContainsKey(ACustomHeaderKey));
				Assert.False(resultWithoutCustomHeaders is FailResultWithCustomHeaders);
			}
		}

		// [SkippableFact]
		public async Task HotPotato_ResultState_ShouldSerializeCorrectly()
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

				IResultCollector resultCollector = servicePro.GetService<IResultCollector>();
				resultCollector.Results.Clear();

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ProxyEndpoint)))
				{
					await client.SendAsync(req);
				}

				using (HotPotatoRequest req = new HotPotatoRequest(method, new System.Uri(ResultsEndpoint)))
				{
					var response = await client.SendAsync(req);
					string result = response.ToBodyString();
					Assert.Contains(SerializedState, result);
				}
			}
		}
	}
}
