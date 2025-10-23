using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create
{
	internal class CustomerCreateUseCase : AbstractCreateUseCase<CustomerCreateDto, Customer, int>, ICustomerCreateUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly ICustomerQueryInfrastructureProviderService _customerQueryInfrastructureProviderService;
		private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
		private readonly IValidationEngine _validationEngine;
		private readonly ILogger<CustomerCreateUseCase> _logger;

		public CustomerCreateUseCase(
			ExampleScopedSettings scopedSettings,
			IValidationEngine validationEngine,
			IValidationRuleEngine validationConfiguration,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			ICustomerQueryInfrastructureProviderService customerQueryInfrastructureProviderService,
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService,
			ILogger<CustomerCreateUseCase> logger
		) : base(validationConfiguration)
		{
			_scopedSettings = scopedSettings;
			_validationEngine = validationEngine;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_customerQueryInfrastructureProviderService = customerQueryInfrastructureProviderService;
			_officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerCreateResponse>> CreateAsync(CustomerCreateRequest request)
		{
			try
			{

				var constraintsResult = await CheckValidationConstraintsAsync(request.Customer);
				if (constraintsResult.IsFaulty)
				{
					return constraintsResult.ConvertTo<CustomerCreateResponse>();
				}

				var createCustomerResult = Customer.CreateEntity(request.Customer, _validationEngine);
				if (createCustomerResult.IsFaulty)
				{
					return createCustomerResult.ConvertTo<CustomerCreateResponse>();
				}

				var createOfficesResult = await CreateOfficesAsync(createCustomerResult.Data, request.Customer.Offices);
				if (createOfficesResult.IsFaulty)
				{
					return createOfficesResult.ConvertTo<CustomerCreateResponse>();
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(createCustomerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerCreateResponse>();
				}

				return new CustomerCreateResponse
				{
					Id = createCustomerResult.Data.Id.Value
				}.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Customer could not be created", ex, additional: new
				{
					request.Customer
				});

				return ResponseData<CustomerCreateResponse>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex, messageGuid);
			}
		}

		public ResponseData<ValidationConfigurationResponse> GetValidationConfiguration()
		{
			return GetValidationConfiguration<CustomerCreateDto>(true);
		}

		private async Task<ResponseData<bool>> CreateOfficesAsync(Customer customer, IEnumerable<CustomerCreateOfficeDto> offices)
		{
			foreach (var office in offices)
			{
				var createResult = await CreateOfficeAsync(customer, office);
				if (createResult.IsFaulty)
				{
					return createResult.ConvertTo<bool>();
				}
			}

			return true.ToResponseData();
		}

		private async Task<ResponseData<Office>> CreateOfficeAsync(Customer customer, CustomerCreateOfficeDto office)
		{
			var constraintsResult = await CheckValidationConstraintsAsync(office);
			if (constraintsResult.IsFaulty)
			{
				return constraintsResult.ConvertTo<Office>();
			}

			return customer.AddOffice(office);
		}

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(CustomerCreateDto customer)
		{
			var isUniqueNameResult = await _customerQueryInfrastructureProviderService.IsUniqueNameAsync(null, customer.Name);
			if (isUniqueNameResult.IsFaulty)
			{
				return isUniqueNameResult;
			}

			if (!isUniqueNameResult.Data)
			{
				return ResponseData<bool>.CreateInvalidDataResponse()
					.AddValidationError(nameof(Customer.Name), "Unique", customer.Name);
			}

			return true.ToResponseData();
		}

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(CustomerCreateOfficeDto office)
		{
			var isUniqueNameResult = await _officeQueryInfrastructureProviderService.IsUniqueNameAsync(null, null, office.Name);
			if (isUniqueNameResult.IsFaulty)
			{
				return isUniqueNameResult;
			}

			if (!isUniqueNameResult.Data)
			{
				return ResponseData<bool>.CreateInvalidDataResponse()
					.AddValidationError(nameof(Office.Name), "Unique", office.Name);
			}

			return true.ToResponseData();
		}
	}
}
