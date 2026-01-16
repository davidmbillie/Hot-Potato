using HotPotato.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag;
using static NSwag.OpenApiYamlDocument;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HotPotato.OpenApi.SpecificationProvider
{
	public class SpecificationProvider : ISpecificationProvider
	{
		private readonly string SpecLocation;
		private readonly string SpecToken;
		private readonly bool ignoreClientCertificateValidationErrors;

		private ILogger Logger { get; }

		public SpecificationProvider(IConfiguration config, ILogger<SpecificationProvider> logger)
		{
			_ = config ?? throw Exceptions.ArgumentNull(nameof(config));
			_ = logger ?? throw Exceptions.ArgumentNull(nameof(logger));

			this.SpecLocation = config["SpecLocation"];
			this.SpecToken = config["SpecToken"];
			this.Logger = logger;
			//mirror the security setting used at startup
			this.ignoreClientCertificateValidationErrors = Convert.ToBoolean(config.GetSection("HttpClientSettings")["IgnoreClientHttpsCertificateValidationErrors"]);

			Logger.LogInformation($"Spec located at {SpecLocation}");
		}
		public OpenApiDocument GetSpecDocument()
		{
			Task<OpenApiDocument> swagTask;
			if (Uri.IsWellFormedUriString(SpecLocation, UriKind.Absolute))
			{
				swagTask = FromUrlAsyncWithClient(SpecLocation);
			}
			else if (File.Exists(SpecLocation))
			{
				swagTask = FromFileAsync(SpecLocation);
			}
			else
			{
				throw Exceptions.InvalidOperation("AppSettings does not contain a valid Spec Location");
			}
			return swagTask.Result;
		}

		private async Task<OpenApiDocument> FromUrlAsyncWithClient(string url)
		{
			using (HttpClientHandler handler = new HttpClientHandler())
			{
				if (ignoreClientCertificateValidationErrors)
				{
					handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) =>
					{
						//output which certificates are being accepted
						Console.WriteLine(certificate);
						return true;
					};
				}
				using (HttpClient client = new HttpClient(handler))
				{
					client.BaseAddress = new Uri(url);
					using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
					{
						if (!string.IsNullOrWhiteSpace(SpecToken))
						{
							req.Headers.Add("Authorization", $"Bearer {SpecToken}");
						}
						using (HttpResponseMessage response = await client.SendAsync(req).ConfigureAwait(false))
						{
							if (!response.IsSuccessStatusCode)
							{
								Logger.LogDebug($"Failed to retrieve spec. Response: {response}");
								throw Exceptions.SpecNotFound(SpecLocation, response);
							}

							string data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
							return await FromYamlAsync(data, url).ConfigureAwait(false);
						}
					}
				}
			}
		}
	}
}
