using HotPotato.Test.Api.Models;
using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using HotPotato.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HotPotato.TestServ.Test
{
	public class RawPotatoTest : IClassFixture<TestFixture<HotPotato.Test.Api.Startup>>, IDisposable
	{
		private HotPotatoClient client;
		private List<Result> results;

		private readonly Order paperOrder = new Order()
		{
			Id = 5,
			Price = 10.00,
			Items = new List<Item>()
			{
				new Item()
				{
					ItemId = 1,
					Name = "Paper",
					Price = 10.00
				}
			}
		};

		public RawPotatoTest(TestFixture<HotPotato.Test.Api.Startup> fixture)
		{
			client = fixture.Client;
			results = fixture.Results;
		}

		[SkippableTheory]
		[InlineData("http://localhost:3232", "GET", 200)]
		[InlineData("http://localhost:3232/order", "GET", 200)]
		[InlineData("http://localhost:3232/order", "POST", 201, true)]
		[InlineData("http://localhost:3232/order/3", "PUT", 204, true)]
		[InlineData("http://localhost:3232/order/1/items/3", "DELETE", 204)]
		[InlineData("http://localhost:3232/order/2", "OPTIONS", 200)]
		[InlineData("http://localhost:3232/order/4", "GET", 200)]
		public async Task HotPotato_Should_Process_RawPotato_HappyPaths(string path, string methodString, int expectedStatusCode, bool hasRequestBody = false)
		{
			HttpMethod method = new HttpMethod(methodString);
			Uri pathUri = new Uri(path);

			using (HotPotatoRequest req = new HotPotatoRequest(method, pathUri))
			{
				if (hasRequestBody)
				{
					string paperOrderBody = JsonConvert.SerializeObject(paperOrder);
					req.SetContent(paperOrderBody);
				}
				await client.SendAsync(req);

				Result result = results.ElementAt(0);

				Assert.True(result.State == State.Pass, result.ToString());
				Assert.Equal(methodString, result.Method, ignoreCase: true);
				Assert.Equal(pathUri.AbsolutePath, result.Path);
				Assert.Equal(expectedStatusCode, result.StatusCode);
			}
		}

		[SkippableTheory]
		[InlineData("http://localhost:3232", "GET")]
		[InlineData("http://localhost:3232/order", "GET")]
		[InlineData("http://localhost:3232/expected_fail", "GET")]
		[InlineData("http://localhost:3232/order/2", "OPTIONS")]
		[InlineData("http://localhost:3232/order/4", "GET")]
		public async Task One_Fail_Result_Should_Not_Fail_The_Whole_Set(string path, string methodString)
		{
			HttpMethod method = new HttpMethod(methodString);
			Uri pathUri = new Uri(path);

			using (HotPotatoRequest req = new HotPotatoRequest(method, pathUri))
			{
				await client.SendAsync(req);

				Result result = results.ElementAt(0);

				if (path.Contains("expected_fail"))
				{
					Assert.True(result.State == State.Fail, result.ToString());
				}
				else
				{
					Assert.True(result.State == State.Pass, result.ToString());
				}
			}
		}

		public void Dispose()
		{
			results.Clear();
			GC.SuppressFinalize(this);
		}
	}
}
