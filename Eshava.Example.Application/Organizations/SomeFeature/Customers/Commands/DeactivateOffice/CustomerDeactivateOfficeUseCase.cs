using System;
using System.Net;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.DeactivateOffice
{
	internal class CustomerDeactivateOfficeUseCase : AbstractDeactivateUseCase<Customer, int>, ICustomerDeactivateOfficeUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly ILogger<CustomerDeactivateOfficeUseCase> _logger;

		public CustomerDeactivateOfficeUseCase(
			ExampleScopedSettings scopedSettings,
			IValidationRuleEngine validationConfiguration,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			ILogger<CustomerDeactivateOfficeUseCase> logger
		)
		{
			_scopedSettings = scopedSettings;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerDeactivateOfficeResponse>> DeactivateOfficeAsync(CustomerDeactivateOfficeRequest request)
		{
			try
			{
				var customerResult = await _customerInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerDeactivateOfficeResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerDeactivateOfficeResponse>();
				}

				var processOfficeChangesResult = DeactivateOffice(customerResult.Data, request.OfficeId);
				if (processOfficeChangesResult.IsFaulty)
				{
					return processOfficeChangesResult.ConvertTo<CustomerDeactivateOfficeResponse>();
				}

				if (!customerResult.Data.IsChanged)
				{
					return new CustomerDeactivateOfficeResponse().ToResponseData(HttpStatusCode.NoContent);
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(customerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerDeactivateOfficeResponse>();
				}

				return new CustomerDeactivateOfficeResponse()
					.ToResponseData(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Office could not be deactivated", ex, additional: new
				{
					request.CustomerId,
					request.OfficeId
				});

				return ResponseData<CustomerDeactivateOfficeResponse>.CreateInternalServerError(MessageConstants.DELETEDATAERROR, ex, messageGuid);
			}
		}

		private ResponseData<bool> DeactivateOffice(Customer customer, int officeId)
		{
			var officeResult = customer.GetOffice(officeId);
			if (officeResult.IsFaulty)
			{
				return officeResult.ConvertTo<bool>();
			}

			return officeResult.Data.Deactivate();
		}
	}
}