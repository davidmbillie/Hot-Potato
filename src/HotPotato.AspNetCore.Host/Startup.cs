﻿using HotPotato.Http;
using HotPotato.Http.Default;
using HotPotato.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HotPotato.AspNetCore.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public ILogger Log { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> log)
        {
            _ = configuration ?? throw new ArgumentException(nameof(configuration));
            _ = log ?? throw new ArgumentException(nameof(log));

            Configuration = configuration;
            Log = log;
        }

        public void Configure(ILoggerFactory loggerFactory, IApplicationBuilder builder, IHostingEnvironment env)
        {
            builder.UseResponseBuffering();
            builder.UseMiddleware<Middleware.HotPotatoMiddleware>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IProxy, HotPotato.Proxy.Default.Proxy>();
            services.AddScoped<IHttpClient, HttpClient>();
            services.AddHttpClient<IHttpClient, HttpClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration["RemoteEndpoint"]);
            });
        }
    }
}
