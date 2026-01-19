using HotPotato.AspNetCore.Middleware;
using HotPotato.Core.Http.Default;
using HotPotato.OpenApi.Models;
using HotPotato.OpenApi.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;

namespace HotPotato.TestServ.Test
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {
        public HotPotatoClient Client { get; }
        public List<Result> Results { get; }
        public bool SpecTokenExists { get; }

        private const string ApiServerAddress = "http://localhost:5000";
        private const string HotPotatoAddress = "http://localhost:3232";

        private readonly TestServer apiServer;
        private readonly TestServer hotPotatoServer;

        public TestFixture()
        {
            var apiHostBuilder = new HostBuilder()
                .ConfigureWebHost(web =>
                {
                    web.UseTestServer();
                    web.UseStartup<TStartup>();
                });

            var apiHost = apiHostBuilder.Start();
            apiServer = apiHost.GetTestServer();
            apiServer.BaseAddress = new Uri(ApiServerAddress);

            var apiClient = new HotPotatoClient(apiServer.CreateClient());

            var hotPotatoHostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                          .AddJsonFile("appsettings.json", optional: true)
                          .AddEnvironmentVariables()
                          .AddUserSecrets<TestFixture<TStartup>>();

                    // Equivalent of old UseSetting("RemoteEndpoint", ...)
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["RemoteEndpoint"] = ApiServerAddress
                    });
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddDebug();
                    }
                })
                .ConfigureServices(services =>
                {
                    // Your existing extension method
                    services.ConfigureMiddlewareServices(apiClient);
                })
                .ConfigureWebHost(web =>
                {
                    web.UseTestServer();
                    web.Configure(app =>
                    {
                        app.UseMiddleware<HotPotatoMiddleware>();
                    });
                });

            var hotPotatoHost = hotPotatoHostBuilder.Start();
            hotPotatoServer = hotPotatoHost.GetTestServer();
            hotPotatoServer.BaseAddress = new Uri(HotPotatoAddress);

            // ---------------------------------------------------------
            // Resolve test services
            // ---------------------------------------------------------
            Results = hotPotatoHost.Services.GetRequiredService<IResultCollector>().Results;
            Client = new HotPotatoClient(hotPotatoServer.CreateClient());

            var configuration = hotPotatoHost.Services.GetRequiredService<IConfiguration>();
            if (!string.IsNullOrWhiteSpace(configuration["SpecToken"]))
            {
                SpecTokenExists = true;
            }
        }

        public void Dispose()
        {
            apiServer.Dispose();
            hotPotatoServer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
