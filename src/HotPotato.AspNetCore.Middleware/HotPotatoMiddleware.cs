using HotPotato.Core;
using HotPotato.Core.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HotPotato.Core.Models;
using HotPotato.OpenApi;
using HotPotato.Core.Processor;
using HotPotato.Core.Http;

namespace HotPotato.AspNetCore.Middleware
{
	public class HotPotatoMiddleware
	{
		private const string SpecLocationKey = "SpecLocation";

		private IProcessor Processor { get; }
		private ILogger Log { get; }
		private RequestDelegate Next { get; }

		public HotPotatoMiddleware(RequestDelegate next, IConfiguration configuration, IProcessor processor, ILogger<HotPotatoMiddleware> log)
		{
			Next = next ?? throw Exceptions.ArgumentNull(nameof(next));
			_ = configuration ?? throw Exceptions.ArgumentNull(nameof(configuration));
			Processor = processor ?? throw Exceptions.ArgumentNull(nameof(processor));
			Log = log ?? throw Exceptions.ArgumentNull(nameof(log));

			_ = configuration[SpecLocationKey] ?? throw Exceptions.InvalidOperation("'SpecLocation' is not defined");
		}

		public async Task Invoke(HttpContext context)
		{
			IHotPotatoRequest request = await context.Request.ToHotPotatoRequest();
			await Next(context);
			IHotPotatoResponse response = await context.Response.ToHotPotatoResponse();

			HttpPair httpPair = new HttpPair(request, response);

			Processor.Process(httpPair);
		}
	}
}
