using System;
using System.Net;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Deactivate
{
	internal class CustomerDeactivateUseCase : AbstractDeactivateUseCase<Customer, int>, ICustomerDeactivateUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly ILogger<CustomerDeactivateUseCase> _logger;

		public CustomerDeactivateUseCase(
			ExampleScopedSettings scopedSettings,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			ICustomerQueryInfrastructureProviderService customerQueryInfrastructureProviderService,
			ILogger<CustomerDeactivateUseCase> logger
		)
		{
			_scopedSettings = scopedSettings;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerDeactivateResponse>> DeactivateAsync(CustomerDeactivateRequest request)
		{
			try
			{
				var customerResult = await _customerInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerDeactivateResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerDeactivateResponse>();
				}

				var isDeletableResult = await IsDeletableAsync(customerResult.Data);
				if (isDeletableResult.IsFaulty)
				{
					return isDeletableResult.ConvertTo<CustomerDeactivateResponse>();
				}

				var deactivateResult = customerResult.Data.Deactivate();
				if (deactivateResult.IsFaulty)
				{
					return deactivateResult.ConvertTo<CustomerDeactivateResponse>();
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(customerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerDeactivateResponse>();
				}

				return new CustomerDeactivateResponse()
					.ToResponseData(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Customer could not be deactivated", ex, additional: new
				{
					request.CustomerId
				});

				return ResponseData<CustomerDeactivateResponse>.CreateInternalServerError(MessageConstants.DELETEDATAERROR, ex, messageGuid);
			}
		}
	}
}