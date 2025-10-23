using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Enums;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.Example.Application.Settings;
using Eshava.Storm;
using Eshava.Storm.MetaData;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerRepository : AbstractExampleDomainModelRepository<Domain.Organizations.SomeFeature.Customer, Customer, int, ExampleScopedSettings>, ICustomerRepository
	{
		private static readonly List<(Expression<Func<Customer, object>> Data, Expression<Func<Domain.Organizations.SomeFeature.Customer, object>> Domain)> _customerDataToCustomerDomain = [(p => p.CompanyName, p => p.Name)];
		private static readonly List<(Expression<Func<Offices.Office, object>> Data, Expression<Func<Domain.Organizations.SomeFeature.Office, object>> Domain)> _officeDataToOfficeDomain = [];

		private readonly IValidationEngine _validationEngine;
		private const string CUSTOMER = "customer";
		private const string OFFICE = "office";

		public CustomerRepository(
			IDatabaseSettings databaseSettings,
			ExampleScopedSettings scopedSettings,
			ITransformQueryEngine transformQueryEngine,
			IValidationEngine validationEngine,
			ILogger<CustomerRepository> logger
		) : base(databaseSettings, scopedSettings, transformQueryEngine, logger)
		{
			_validationEngine = validationEngine;
		}

		public async Task<ResponseData<Domain.Organizations.SomeFeature.Customer>> ReadAsync(int customerId)
		{
			try
			{
				var query = $"""

				SELECT
					 {CUSTOMER}.*
					,{OFFICE}.*
				FROM
					 {TypeAnalyzer.GetTableName<Customer>()} {CUSTOMER}
				LEFT JOIN
					 {TypeAnalyzer.GetTableName<Offices.Office>()} {OFFICE}
						ON {OFFICE}.{nameof(Offices.Office.CustomerId)} = {CUSTOMER}.{nameof(Customer.Id)}
						AND {OFFICE}.{nameof(Offices.Office.Status)} = @Status
				WHERE
					{CUSTOMER}.{nameof(Customer.Id)} = @CustomerId
				AND
					{CUSTOMER}.{nameof(Customer.Status)} = @Status
					
				""";

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.QueryAsync<Customer>(query, mapper =>
					{
						var customer = mapper.Map<Customer>(CUSTOMER);
						var office = default(Offices.Office);
						if (customer is not null && mapper.GetValue<int>("Id", OFFICE) > 0)
						{
							office = mapper.Map<Offices.Office>(OFFICE);
							customer.Office = office;
						}

						return customer;
					},
					new { CustomerId = customerId, Status = Status.Active });

					if (!result.Any())
					{
						return new ResponseData<Domain.Organizations.SomeFeature.Customer>(null);
					}

					var customerRawItem = result.First();
					var officeRawItems = result.Where(p => p.Office is not null).Select(p => p.Office).ToList();

					var officeModels = new List<Domain.Organizations.SomeFeature.Office>();
					foreach (var officeItem in officeRawItems)
					{
						var officePatches = GenerateDomainPatchList(officeItem, _officeDataToOfficeDomain);
						var officeModel = Domain.Organizations.SomeFeature.Office.DataToInstance(officePatches, _validationEngine);
						officeModels.Add(officeModel);
					}

					var customerPatches = GenerateDomainPatchList(customerRawItem, _customerDataToCustomerDomain);
					var customerModel = Domain.Organizations.SomeFeature.Customer.DataToInstance(customerPatches, officeModels, _validationEngine);

					return customerModel.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, "Entity Customer could not be read", ex, additional: new
				{
					CustomerId = customerId
				});

				return ResponseData<Domain.Organizations.SomeFeature.Customer>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		protected override Customer FromDomainModel(Domain.Organizations.SomeFeature.Customer model)
		{
			if (model is null)
			{
				return null;
			}

			var instance = new Customer
			{
				CompanyName = model.Name,
				Classification = model.Classification
			};

			return FromDomainModel(instance, model);
		}

		protected override string GetPropertyName(Patch<Domain.Organizations.SomeFeature.Customer> patch)
		{
			var mapping = _customerDataToCustomerDomain.FirstOrDefault(p => p.Domain.GetMemberExpressionString() == patch.PropertyName);
			if (mapping.Domain is not null)
			{
				return mapping.Data.GetMemberExpressionString();
			}

			return base.GetPropertyName(patch);
		}
	}
}