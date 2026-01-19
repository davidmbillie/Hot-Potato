using HotPotato.AspNetCore.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;

namespace HotPotato
{
	public static class IntegrationTestMethods
	{
		/// <summary>
		/// Returns a path with the correct directory separator based on your platform
		/// </summary>
		public static string SpecPath(string subPath, string file)
		{
			string path = Path.Combine(Environment.CurrentDirectory, subPath, file);
			path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			if (File.Exists(path))
			{
				return path;
			}
			else
			{
				return "";
			}
		}

		public static ServiceProvider GetServiceProvider(string specLocation)
		{
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();

			config["SpecLocation"] = specLocation;

			Startup startUp = new Startup(config);

			IServiceCollection services = new ServiceCollection();
			startUp.ConfigureServices(services);

			services.AddSingleton<IConfiguration>(config);
			return services.BuildServiceProvider();
		}
	}
}
