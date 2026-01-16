using HotPotato.Core.Cookies;
using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
using HotPotato.Core.Processor;
using HotPotato.Core.Proxy;
using HotPotato.OpenApi.Results;
using HotPotato.OpenApi.SpecificationProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using System.Collections.Generic;

namespace HotPotato.AspNetCore.Middleware
{
	public class HostExtensionsTest
	{
		private const string SpecLocation = "https://russian.blue.potato.com/projects/TATO/repos/hot-potato/raw/test/RawPotatoSpec.yaml";
		private const string ApiServerAddress = "http://localhost:5000";

		[Fact]
		public void ConfigureMiddlewareServices_SetsAllExpectedServices()
		{
			HotPotatoClient client = new HotPotatoClient(new System.Net.Http.HttpClient());

			var subject = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
					config.AddInMemoryCollection(new Dictionary<string, string>
					{
						["SpecLocation"] = SpecLocation,
						["RemoteEndpoint"]= ApiServerAddress
					});
				})
				.ConfigureServices(services =>
				{
					services.ConfigureMiddlewareServices(client);
				})
				.Build();

			IServiceProvider result = subject.Services;

			//tried to make this into loop of type variables,
			//but the compiler didn't like using variables as Types
			Assert.NotNull(result.GetService<IProxy>());
			Assert.NotNull(result.GetService<IHotPotatoClient>());
			Assert.Equal(client, result.GetService<IHotPotatoClient>());
			Assert.NotNull(result.GetService<ISpecificationProvider>());
			Assert.NotNull(result.GetService<IResultCollector>());
			Assert.NotNull(result.GetService<IProcessor>());
			Assert.NotNull(result.GetService<ICookieJar>());
		}

		[Fact]
		public void ConfigureMiddlewareServices_Creates_HttpClient_Via_DI()
		{
			var subject = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
					config.AddInMemoryCollection(new Dictionary<string, string>
					{
						["SpecLocation"] = SpecLocation
					});
				})
				.ConfigureServices(services =>
				{
					services.ConfigureMiddlewareServices();
				})
				.Build();

			IServiceProvider result = subject.Services;

			Assert.NotNull(result.GetService<IHotPotatoClient>());
		}
	}
}
