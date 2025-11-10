using System;
using System.Threading.Channels;
using Eshava.Core.Linq;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Linq.Models;
using Eshava.Core.Validation;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.Example.Application.HostedServices;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Deactivate;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.DeactivateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Eshava.Example.Application.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			RegisterSettings(services, configuration);
			RegisterServices(services);

			return services;
		}

		public static IServiceCollection RegisterHostedServices(this IServiceCollection services, IConfiguration configuration)
		{
			var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

			services
				.AddHostedService<DomainEventChannelProcessor>()
				;

			return services;
		}

		private static IServiceCollection RegisterServices(IServiceCollection services)
		{
			services
				.AddSingleton<ITransformQueryEngine, TransformQueryEngine>()
				.AddSingleton<IWhereQueryEngine, WhereQueryEngine>()
				.AddSingleton<ISortingQueryEngine, SortingQueryEngine>()
				.AddSingleton<IValidationRuleEngine, ValidationRuleEngine>()
				.AddSingleton(_ => Channel.CreateUnbounded<DomainEvent>(new UnboundedChannelOptions { SingleReader = true }))
				;

			return services
				.AddScoped<ICustomerCreateOfficeUseCase, CustomerCreateOfficeUseCase>()
				.AddScoped<ICustomerCreateUseCase, CustomerCreateUseCase>()
				.AddScoped<ICustomerDeactivateOfficeUseCase, CustomerDeactivateOfficeUseCase>()
				.AddScoped<ICustomerDeactivateUseCase, CustomerDeactivateUseCase>()
				.AddScoped<ICustomerReadUseCase, CustomerReadUseCase>()
				.AddScoped<ICustomerSearchUseCase, CustomerSearchUseCase>()
				.AddScoped<ICustomerUpdateOfficeUseCase, CustomerUpdateOfficeUseCase>()
				.AddScoped<ICustomerUpdateUseCase, CustomerUpdateUseCase>()
				.AddScoped<IOfficeReadUseCase, OfficeReadUseCase>()
				.AddScoped<IOfficeSearchUseCase, OfficeSearchUseCase>()
				;
		}

		private static void RegisterSettings(IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton(new WhereQueryEngineOptions
			{
				UseUtcDateTime = true,
				ContainsSearchSplitBySpace = true
			});

			services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

			services.AddScoped(provider =>
			{
				var settings = provider.GetService<IOptions<AppSettings>>().Value;
				var scopedSettings = new ExampleScopedSettings
				{
					UserId = DateTime.Now.Millisecond
				};

				return scopedSettings;
			});
		}
	}
}