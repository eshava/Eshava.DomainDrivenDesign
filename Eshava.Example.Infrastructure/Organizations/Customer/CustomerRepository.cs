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

		private static Dictionary<string, Func<object, object>> _customerPropertyValueToDomainMappings = [];
		private static Dictionary<string, Func<object, object>> _officePropertyValueToDomainMappings = [];

		private readonly IValidationEngine _validationEngine;
		private const string CUSTOMER = "customer";
		private const string OFFICE = "office";

		static CustomerRepository()
		{
			PropertyValueToDataMappings = new Dictionary<string, Func<object, object>>
			{
				{ "Example", domainValue => domainValue }
			};
		}

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
						var officePatches = GenerateDomainPatchList(officeItem, _officeDataToOfficeDomain, _officePropertyValueToDomainMappings, CreateValueObjects, _validationEngine);
						var officeModel = Domain.Organizations.SomeFeature.Office.DataToInstance(officePatches, _validationEngine);
						officeModels.Add(officeModel);
					}

					var customerPatches = GenerateDomainPatchList(customerRawItem, _customerDataToCustomerDomain, _customerPropertyValueToDomainMappings, CreateValueObjects, _validationEngine);
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
				/* Example value mapping from domain to data */
				CompanyName = PropertyValueToDataMappings.TryGetValue(nameof(Customer.CompanyName), out var companyNameMappged)
					? (string)companyNameMappged(model.Name)
					: model.Name,
				Classification = model.Classification
			};

			if (model.Address is not null)
			{
				instance.AddressStreet = model.Address.Street;
				instance.AddressStreetNumber = model.Address.StreetNumber;
				instance.AddressCity = model.Address.City;
				instance.AddressZipCode = model.Address.ZipCode;
				instance.AddressCountry = model.Address.Country;
			}

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

		protected override void MapValueObjects(IEnumerable<Patch<Domain.Organizations.SomeFeature.Customer>> patches, IDictionary<string, object> dataModelChanges)
		{
			foreach (var patch in patches)
			{
				if (patch.PropertyName != nameof(Domain.Organizations.SomeFeature.Customer.Address))
				{
					continue;
				}

				var address = patch.Value as Domain.Organizations.SomeFeature.Address;
				if (address is null)
				{
					dataModelChanges.Add(nameof(Customer.AddressStreet), null);
					dataModelChanges.Add(nameof(Customer.AddressStreetNumber), null);
					dataModelChanges.Add(nameof(Customer.AddressCity), null);
					dataModelChanges.Add(nameof(Customer.AddressZipCode), null);
					dataModelChanges.Add(nameof(Customer.AddressCountry), null);
				}
				else
				{
					/* Example value mapping from domain to data */
					dataModelChanges.Add(nameof(Customer.AddressStreet), PropertyValueToDataMappings.TryGetValue(nameof(Customer.AddressStreet), out var addressStreetMappged)
						? (string)addressStreetMappged(address.Street)
						: address.Street);
					dataModelChanges.Add(nameof(Customer.AddressStreetNumber), address.StreetNumber);
					dataModelChanges.Add(nameof(Customer.AddressCity), address.City);
					dataModelChanges.Add(nameof(Customer.AddressZipCode), address.ZipCode);
					dataModelChanges.Add(nameof(Customer.AddressCountry), address.Country);
				}
			}
		}

		private static IEnumerable<Patch<Domain.Organizations.SomeFeature.Customer>> CreateValueObjects(Customer dataInstance, IValidationEngine validationEngine)
		{
			var address = new Domain.Organizations.SomeFeature.Address(
				/* Example value mapping from data to domain  */
				_customerPropertyValueToDomainMappings.TryGetValue(nameof(dataInstance.AddressStreet), out var addressStreetMapped)
					? (string)addressStreetMapped(dataInstance.AddressStreet)
					: dataInstance.AddressStreet,
				dataInstance.AddressStreetNumber,
				dataInstance.AddressCity,
				dataInstance.AddressZipCode,
				dataInstance.AddressCountry
			);

			var addressValidationResult = validationEngine.Validate(address);

			return addressValidationResult.IsValid
				? [Patch<Domain.Organizations.SomeFeature.Customer>.Create(p => p.Address, address)]
				: [];
		}

		private static IEnumerable<Patch<Domain.Organizations.SomeFeature.Office>> CreateValueObjects(Offices.Office dataInstance, IValidationEngine validationEngine)
		{
			var address = new Domain.Organizations.SomeFeature.Address(
				dataInstance.AddressStreet,
				dataInstance.AddressStreetNumber,
				dataInstance.AddressCity,
				dataInstance.AddressZipCode,
				dataInstance.AddressCountry
			);

			var addressValidationResult = validationEngine.Validate(address);

			return addressValidationResult.IsValid
				? [Patch<Domain.Organizations.SomeFeature.Office>.Create(p => p.Address, address)]
				: [];
		}
	}
}