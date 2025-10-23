using System;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice
{
	internal class CustomerCreateOfficeUseCase : AbstractCreateUseCase<CustomerCreateOfficeDto, Customer, int>, ICustomerCreateOfficeUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
		private readonly ILogger<CustomerCreateOfficeUseCase> _logger;

		public CustomerCreateOfficeUseCase(
			ExampleScopedSettings scopedSettings,
			IValidationRuleEngine validationConfiguration,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService,
			ILogger<CustomerCreateOfficeUseCase> logger
		) : base(validationConfiguration)
		{
			_scopedSettings = scopedSettings;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerCreateOfficeResponse>> CreateOfficeAsync(CustomerCreateOfficeRequest request)
		{
			try
			{
				var customerResult = await _customerInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerCreateOfficeResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerCreateOfficeResponse>();
				}

				var createOfficesResult = await CreateOfficeAsync(customerResult.Data, request.Office);
				if (createOfficesResult.IsFaulty)
				{
					return createOfficesResult.ConvertTo<CustomerCreateOfficeResponse>();
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(customerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerCreateOfficeResponse>();
				}

				return new CustomerCreateOfficeResponse
				{
					Id = createOfficesResult.Data.Id.Value
				}.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Office could not be created", ex, additional: new
				{
					request.CustomerId,
					request.Office
				});

				return ResponseData<CustomerCreateOfficeResponse>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex, messageGuid);
			}
		}

		public ResponseData<ValidationConfigurationResponse> GetValidationConfiguration()
		{
			return GetValidationConfiguration<CustomerCreateOfficeDto>(true);
		}

		private async Task<ResponseData<Office>> CreateOfficeAsync(Customer customer, CustomerCreateOfficeDto office)
		{
			var constraintsResult = await CheckValidationConstraintsAsync(office, customer.Id.Value);
			if (constraintsResult.IsFaulty)
			{
				return constraintsResult.ConvertTo<Office>();
			}

			return customer.AddOffice(office);
		}

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(CustomerCreateOfficeDto office, int customerId)
		{
			var isUniqueNameResult = await _officeQueryInfrastructureProviderService.IsUniqueNameAsync(customerId, null, office.Name);
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