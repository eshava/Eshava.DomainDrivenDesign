﻿using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Settings;
using Eshava.DomainDrivenDesign.Infrastructure.Storm;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Infrastructure.Organizations;
using Eshava.Example.Infrastructure.Organizations.Customers;
using Eshava.Example.Infrastructure.Organizations.Offices;
using Eshava.Storm.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eshava.Example.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			var msSqlConnectionString = configuration.GetConnectionString("Default");
			if (!msSqlConnectionString.IsNullOrEmpty())
			{
				services.AddScoped<IDatabaseSettings>(_ => new DatabaseSettings(msSqlConnectionString));
			}

			AddDatabaseConfigurations();

			return services
				.AddRepositories()
				.AddProviders()
				;
		}

		private static IServiceCollection AddProviders(this IServiceCollection services)
		{
			return services
				.AddScoped<ICustomerQueryInfrastructureProviderService, CustomerQueryInfrastructureProviderService>()
				.AddScoped<ICustomerInfrastructureProviderService, CustomerInfrastructureProviderService>()
				.AddScoped<IOfficeQueryInfrastructureProviderService, OfficeQueryInfrastructureProviderService>()
				;
		}

		private static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			return services
				.AddScoped<ICustomerQueryRepository, CustomerQueryRepository>()
				.AddScoped<ICustomerRepository, CustomerRepository>()
				.AddScoped<IOfficeQueryRepository, OfficeQueryRepository>()
				.AddScoped<IOfficeRepository, OfficeRepository>()
				;
		}

		private static void AddDatabaseConfigurations()
		{
			new DateTimeHandler().AddTypeHandler();
			new DateOnlyHandler().AddTypeHandler();
			new TimeOnlyHandler().AddTypeHandler();

#if DEBUG
			Eshava.Storm.Settings.RestrictToRegisteredModels = true;
#endif

			// Register domain models
			new OrganizationsTransformProfile();

			// Register db configurations
			Eshava.Storm.MetaData.TypeAnalyzer.AddType(new CustomerDbConfiguration());
			Eshava.Storm.MetaData.TypeAnalyzer.AddType(new OfficeDbConfiguration());
		}
	}
}