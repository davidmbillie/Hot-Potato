using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotPotato.AspNetCore.Host
{
    // This class exists ONLY for TestServer / WebApplicationFactory compatibility.
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Delegate to Program.cs shared method
            Program.ConfigureServices(
                services,
                _configuration,
                new LoggingBuilder(services));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Delegate to Program.cs shared method
            Program.ConfigurePipeline(app, env);
        }

        // Minimal wrapper to satisfy Program.ConfigureServices signature
        private class LoggingBuilder : ILoggingBuilder
        {
            public IServiceCollection Services { get; }

            public LoggingBuilder(IServiceCollection services)
            {
                Services = services;
            }
        }
    }
}
