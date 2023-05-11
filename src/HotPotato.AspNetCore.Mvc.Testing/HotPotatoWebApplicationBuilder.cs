﻿using HotPotato.AspNetCore.Middleware;
using HotPotato.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace HotPotato.AspNetCore.Mvc.Testing;

public class HotPotatoWebApplicationBuilder<TProgram>
	: WebApplicationFactory<TProgram> where TProgram : class
{
	public override IServiceProvider Services => base.Services;

	protected override IHost CreateHost(IHostBuilder builder)
	{
		
		return base.CreateHost(builder);
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			services.AddHotPotatoServices();
			
		});
		builder.Configure(app =>
		{
			app.UseMiddleware<HotPotatoMiddleware>();
		});
		base.ConfigureWebHost(builder);
	}
	protected override void ConfigureClient(HttpClient client)
	{
		
	}
}