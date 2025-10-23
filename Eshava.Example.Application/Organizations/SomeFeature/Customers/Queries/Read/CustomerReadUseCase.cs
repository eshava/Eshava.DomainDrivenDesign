using System;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	internal class CustomerReadUseCase : AbstractReadUseCase<CustomerReadRequest, CustomerReadDto>, ICustomerReadUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerQueryInfrastructureProviderService _customerQueryInfrastructureProviderService;
		private readonly ILogger<CustomerReadUseCase> _logger;

		public CustomerReadUseCase(
			ExampleScopedSettings scopedSettings,
			ICustomerQueryInfrastructureProviderService customerQueryInfrastructureProviderService,
			ILogger<CustomerReadUseCase> logger
			)
		{
			_scopedSettings = scopedSettings;
			_customerQueryInfrastructureProviderService = customerQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerReadResponse>> ReadAsync(CustomerReadRequest request)
		{
			try
			{
				var customerResult = await _customerQueryInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerReadResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerReadResponse>();
				}

				customerResult = await AdjustDtoAsync(request, customerResult.Data);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerReadResponse>();
				}

				return new CustomerReadResponse
				{
					Customer = customerResult.Data
				}.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Customer could not be read", ex, additional: new
				{
					request.CustomerId
				});

				return ResponseData<CustomerReadResponse>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}
	}
}