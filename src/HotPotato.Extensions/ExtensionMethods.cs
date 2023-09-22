using HotPotato.Core.Http.Default;
using HotPotato.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HotPotato.Core.Proxy;
using HotPotato.Core.Http;
using HotPotato.OpenApi.SpecificationProvider;
using HotPotato.OpenApi.Results;
using HotPotato.Core.Processor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using HotPotato.OpenApi.Processor;

namespace HotPotato.Extensions
{
	public static class ExtensionMethods
	{
		public static IServiceCollection AddHotPotatoForwardProxy(this IServiceCollection @this)
		{
			@this.AddScoped<IProxy, HotPotato.Core.Proxy.Default.Proxy>();
			@this.AddScoped<IHotPotatoClient, HotPotatoClient>();
			return @this;
		}

		public static IServiceCollection AddHotPotatoServices(this IServiceCollection @this)
		{
			@this.AddSingleton<ISpecificationProvider, SpecificationProvider>();
			@this.AddSingleton<IResultCollector, ResultCollector>();

			@this.AddTransient<IProcessor, Processor>();
			return @this;
		}

		public static TestServer SetupHotPotatoServer(this TestServer apiServer, string testServerAddress)
		{
			HotPotatoClient apiClient = new HotPotatoClient(apiServer.CreateClient());

			IWebHostBuilder hotPotatoBuilder = new WebHostBuilder()
			//Setting this here instead of in appsettings.json so it always matches the BaseAddress on TestServer
			.UseSetting("RemoteEndpoint", testServerAddress)
			.ConfigureAppConfiguration((hostingContext, config) =>
			{
				config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
					.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
			})
			.ConfigureLogging((hostingContext, logging) =>
			{
				logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
				logging.AddConsole();
				if (hostingContext.HostingEnvironment.EnvironmentName == Microsoft.Extensions.Hosting.Environments.Development)
				{
					logging.AddDebug();
				}
			})
			.ConfigureServices(services =>
			{
				services.ConfigureMiddlewareServices(apiClient);
			})
			.Configure(builder =>
			{
				builder.UseMiddleware<HotPotatoMiddleware>();
			});

			return new TestServer(hotPotatoBuilder);
		}
	}
}
