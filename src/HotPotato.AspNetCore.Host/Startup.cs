using HotPotato.AspNetCore.Middleware;
using HotPotato.Core;
using HotPotato.Core.Http;
using HotPotato.Core.Http.ForwardProxy;
using HotPotato.Core.Processor;
using HotPotato.Core.Proxy;
using HotPotato.OpenApi.Results;
using HotPotato.OpenApi.Processor;
using HotPotato.OpenApi.SpecificationProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace HotPotato.AspNetCore.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public ILogger Log { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> log)
        {
            _ = configuration ?? throw Exceptions.ArgumentNull(nameof(configuration));
            _ = log ?? throw Exceptions.ArgumentNull(nameof(log));

            Configuration = configuration;
            Log = log;
        }

        public void Configure(ILoggerFactory loggerFactory, IApplicationBuilder builder, IHostingEnvironment env)
        {
            builder.UseResponseBuffering();
            builder.UseMiddleware<HotPotatoMiddleware>();
            builder.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IProxy, HotPotato.Core.Proxy.Default.Proxy>();
            services.AddScoped<IHttpClient, HotPotato.Core.Http.Default.HttpClient>();
            services.AddMvcCore().AddJsonFormatters();
            services.AddSingleton<HttpForwardProxyConfig>(Configuration.GetSection("ForwardProxy").Get<HttpForwardProxyConfig>());
            services.AddSingleton<IWebProxy, Core.Http.ForwardProxy.Default.HttpForwardProxy>();
            services.AddSingleton<System.Net.Http.HttpClientHandler, System.Net.Http.HttpClientHandler>();
            services.AddHttpClient<IHttpClient, HotPotato.Core.Http.Default.HttpClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration["RemoteEndpoint"]);                
            });
            services.AddSingleton<ISpecificationProvider, SpecificationProvider>();
            services.AddSingleton<IResultCollector, ResultCollector>();

            services.AddTransient<IProcessor, Processor>();
        }
    }
}
