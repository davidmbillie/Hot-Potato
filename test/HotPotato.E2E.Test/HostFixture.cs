﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HotPotato.AspNetCore.Host;
using System;

namespace HotPotato.E2E.Test
{
    public class HostFixture : IDisposable
    {
        public IWebHost host { get; }
        
        private const string ApiLocation = "http://localhost:5000";

        private const string SpecLocation = "https://bitbucket.hylandqa.net/projects/AUTOTEST/repos/hot-potato/raw/test/RawPotatoSpec.yaml?at=refs%2Fheads%2Ffeat%2FAUTOTEST-371-content-is-null-causing-a-nullreferenceexception";

        public HostFixture()
        {
            host = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true);
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
                .UseKestrel((options) =>
                {
                    options.AddServerHeader = false;
                })
                .UseSetting("RemoteEndpoint", ApiLocation)
                .UseSetting("SpecLocation", SpecLocation)
                .UseUrls("http://0.0.0.0:3232")
                .UseStartup<Startup>()
            .Build();

            host.Start();
        }

        public void Dispose()
        {
            host.Dispose();
        }
    }
}
