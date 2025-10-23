﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Eshava.DomainDrivenDesign.Application.PartialPut;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Api.Middleware;
using Eshava.Example.Application.Extensions;
using Eshava.Example.Application.Logging;
using Eshava.Example.Domain.Extensions;
using Eshava.Example.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			Configure(builder.Services, builder.Environment, builder.Configuration);

			var app = builder.Build();

			Configure(app, builder.Environment);

			app.Run();
		}

		private static void Configure(IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
		{
			services
				.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
				;

			services.ConfigureHttpJsonOptions(options =>
			{
				options.SerializerOptions.Converters.Add(new PartialPutDocumentConverterTextJsonFactory());
				options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			});

			services.AddHttpContextAccessor();

			services
				.AddDomain(configuration, environment)
				.AddInfrastructure(configuration, environment)
				.AddApplication(configuration, environment)
				;
						
			services
				.RegisterHostedServices(configuration);
		}

		private static void Configure(WebApplication app, IWebHostEnvironment environment)
		{
			if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
			{
				app.UseDeveloperExceptionPage();
			}

			var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
			AddLoggingProvider(app, environment, loggerFactory);
			app.UseStaticFiles();
			app.UseRouting();
			app.UseCors("AllowAll");

			new Endpoints.CustomerEndpoints().Map(app);
			new Endpoints.OfficeEndpoints().Map(app);
		}

		private static void AddLoggingProvider(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddProvider(new DummyLoggerProvider());

			app.UseMiddleware<LogExceptionMiddleware>();

			loggerFactory.CreateLogger<Program>().LogInformation(new Program(), null, "Starting", additional: new { env.ApplicationName, env.EnvironmentName });

			app.Lifetime.ApplicationStopping.Register(() =>
			{
				loggerFactory.CreateLogger<Program>().LogInformation(new Program(), null, "Stopping");
			});
		}
	}
}