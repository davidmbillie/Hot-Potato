using HotPotato.AspNetCore.Middleware;
using HotPotato.Core;
using HotPotato.Core.Cookies;
using HotPotato.Core.Http;
using HotPotato.Core.Http.Default;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;

namespace HotPotato.AspNetCore.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------------------------------------------------------
            // Configuration
            // ---------------------------------------------------------
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>();

            // ---------------------------------------------------------
            // Logging
            // ---------------------------------------------------------
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Logging.AddConsole();
			builder.WebHost.UseUrls("http://0.0.0.0:3232");

            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.AddDebug();
            }

            // ---------------------------------------------------------
            // Services
            // ---------------------------------------------------------
            ConfigureServices(builder.Services, builder.Configuration, builder.Logging);

            // ---------------------------------------------------------
            // Build + Pipeline
            // ---------------------------------------------------------
            var app = builder.Build();

            ConfigurePipeline(app, app.Environment);

            app.Run();
        }

        // ---------------------------------------------------------
        // Shared service registration (used by Program + Startup shim)
        // ---------------------------------------------------------
        public static void ConfigureServices(
            IServiceCollection services,
            IConfiguration config,
            ILoggingBuilder logging)
        {
            bool ignoreClientCertificateValidationErrors =
                config.GetSection("HttpClientSettings")
                      .GetValue<bool>("IgnoreClientHttpsCertificateValidationErrors");

            LogTlsValidationSetting(ignoreClientCertificateValidationErrors, logging);

            services.AddScoped<IProxy, HotPotato.Core.Proxy.Default.Proxy>();
            services.AddScoped<IHotPotatoClient, HotPotatoClient>();

            services.AddMvcCore().AddNewtonsoftJson();

            services.AddSingleton<IWebProxy, Core.Http.ForwardProxy.Default.HttpForwardProxy>();
            services.AddSingleton(config.GetSection("ForwardProxy").Get<HttpForwardProxyConfig>());
            services.AddSingleton<ICookieJar, CookieJar>();

            services.AddHttpClient<IHotPotatoClient, HotPotatoClient>()
                .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    Proxy = sp.GetRequiredService<HttpForwardProxyConfig>().Enabled
                        ? sp.GetRequiredService<IWebProxy>()
                        : null,
                    CookieContainer = sp.GetRequiredService<ICookieJar>().Cookies,
                    ServerCertificateCustomValidationCallback =
                        ignoreClientCertificateValidationErrors
                            ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                            : null
                });

            services.AddSingleton<ISpecificationProvider, SpecificationProvider>();
            services.AddSingleton<IResultCollector, ResultCollector>();

            services.AddTransient<IProcessor, Processor>();
        }

        // ---------------------------------------------------------
        // Shared pipeline (used by Program + Startup shim)
        // ---------------------------------------------------------
        public static void ConfigurePipeline(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<HotPotatoMiddleware>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // ---------------------------------------------------------
        // TLS Logging helper
        // ---------------------------------------------------------
        private static void LogTlsValidationSetting(bool settingValue, ILoggingBuilder logging)
        {
            using var loggerFactory = LoggerFactory.Create(lb =>
            {
                lb.AddConsole();
                lb.AddDebug();
            });

            var log = loggerFactory.CreateLogger("TLS");

            if (settingValue)
            {
                log.LogWarning(
                    "IgnoreClientCertificateValidation is TRUE. TLS certificate validation errors will be ignored.");
            }
            else
            {
                log.LogInformation(
                    "IgnoreClientCertificateValidation is FALSE. TLS certificate validation errors will cause failures.");
            }
        }
    }
}
