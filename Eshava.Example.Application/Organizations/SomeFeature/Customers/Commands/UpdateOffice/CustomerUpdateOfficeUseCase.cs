using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.Extensions;
using Eshava.DomainDrivenDesign.Application.PartialPut;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice
{
	internal class CustomerUpdateOfficeUseCase : AbstractUpdateUseCase<Customer, CustomerUpdateOfficeDto, int>, ICustomerUpdateOfficeUseCase
	{
		private static List<(Expression<Func<CustomerUpdateOfficeDto, object>> Dto, Expression<Func<Office, object>> Domain)> _mappings = [
			(dto => dto.Address.Street, domain => domain.Address.Street),
			(dto => dto.Address.StreetNumber, domain => domain.Address.StreetNumber),
			(dto => dto.Address.City, domain => domain.Address.City),
			(dto => dto.Address.ZipCode, domain => domain.Address.ZipCode),
			(dto => dto.Address.Country, domain => domain.Address.Country),
		];

		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
		private readonly ILogger<CustomerUpdateOfficeUseCase> _logger;

		public CustomerUpdateOfficeUseCase(
			ExampleScopedSettings scopedSettings,
			IValidationRuleEngine validationConfiguration,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService,
			ILogger<CustomerUpdateOfficeUseCase> logger
		) : base(validationConfiguration)
		{
			_scopedSettings = scopedSettings;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerUpdateOfficeResponse>> UpdateOfficeAsync(CustomerUpdateOfficeRequest request)
		{
			try
			{
				var customerResult = await _customerInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerUpdateOfficeResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerUpdateOfficeResponse>();
				}

				var processOfficeChangesResult = await ProcessOfficeChangesAsync(customerResult.Data, request.OfficeId, request.Office);
				if (processOfficeChangesResult.IsFaulty)
				{
					return processOfficeChangesResult.ConvertTo<CustomerUpdateOfficeResponse>();
				}

				if (!customerResult.Data.IsChanged)
				{
					return new CustomerUpdateOfficeResponse().ToResponseData(HttpStatusCode.NoContent);
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(customerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerUpdateOfficeResponse>();
				}

				return new CustomerUpdateOfficeResponse()
					.ToResponseData(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Office could not be updated", ex, additional: new
				{
					request.CustomerId,
					request.OfficeId,
					request.Office,
				});

				return ResponseData<CustomerUpdateOfficeResponse>.CreateInternalServerError(MessageConstants.UPDATEDATAERROR, ex, messageGuid);
			}
		}

		public ResponseData<ValidationConfigurationResponse> GetValidationConfiguration()
		{
			return GetValidationConfiguration<CustomerUpdateOfficeDto>(true);
		}

		private async Task<ResponseData<bool>> ProcessOfficeChangesAsync(Customer customer, int officeId, PartialPutDocument<CustomerUpdateOfficeDto> officeDocument)
		{
			var officePatches = officeDocument.GetPatchInformation(_mappings);
			if (officePatches.IsFaulty)
			{
				return officePatches.ConvertTo<bool>();
			}

			var officeResult = customer.GetOffice(officeId);
			if (officeResult.IsFaulty)
			{
				return officeResult.ConvertTo<bool>();
			}

			officePatches = officePatches.Data.CheckAndConvertValueObjectPatches(officeResult.Data);
			if (officePatches.IsFaulty)
			{
				return officePatches.ConvertTo<bool>();
			}

			var constraintsResult = await CheckValidationConstraintsAsync(officeResult.Data, officePatches.Data, customer.Id.Value);
			if (constraintsResult.IsFaulty)
			{
				return constraintsResult;
			}

			var officePatchResult = officeResult.Data.Patch(officePatches.Data);
			if (officePatchResult.IsFaulty)
			{
				return officePatchResult;
			}

			return true.ToResponseData();
		}

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(Office office, IList<Patch<Office>> patches, int customerId)
		{
			var namePatch = patches.FirstOrDefault(p => p.PropertyName == nameof(Office.Name));
			if (namePatch?.Value is not null)
			{
				var nameValue = (string)namePatch.Value;
				var isUniqueNameResult = await _officeQueryInfrastructureProviderService.IsUniqueNameAsync(customerId, office.Id, nameValue);
				if (isUniqueNameResult.IsFaulty)
				{
					return isUniqueNameResult;
				}

				if (!isUniqueNameResult.Data)
				{
					return ResponseData<bool>.CreateInvalidDataResponse()
						.AddValidationError(nameof(Office.Name), "Unique", nameValue);
				}
			}

			return true.ToResponseData();
		}
	}
}