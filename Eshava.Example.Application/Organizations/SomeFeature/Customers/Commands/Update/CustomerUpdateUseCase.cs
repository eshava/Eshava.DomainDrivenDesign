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
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Application.Settings;
using Eshava.Example.Domain.Organizations.SomeFeature;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update
{
	internal class CustomerUpdateUseCase : AbstractUpdateUseCase<Customer, CustomerUpdateDto, int>, ICustomerUpdateUseCase
	{
		private static List<(Expression<Func<CustomerUpdateDto, object>> Dto, Expression<Func<Customer, object>> Domain)> _customerMappings = [
			(dto => dto.Street, domain => domain.Address.Street),
			(dto => dto.StreetNumber, domain => domain.Address.StreetNumber),
			(dto => dto.City, domain => domain.Address.City),
			(dto => dto.ZipCode, domain => domain.Address.ZipCode),
			(dto => dto.Country, domain => domain.Address.Country),
		];

		private static List<(Expression<Func<CustomerUpdateOfficeDto, object>> Dto, Expression<Func<Office, object>> Domain)> _officeMappings = [
			(dto => dto.Address.Street, domain => domain.Address.Street),
			(dto => dto.Address.StreetNumber, domain => domain.Address.StreetNumber),
			(dto => dto.Address.City, domain => domain.Address.City),
			(dto => dto.Address.ZipCode, domain => domain.Address.ZipCode),
			(dto => dto.Address.Country, domain => domain.Address.Country),
		];

		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerInfrastructureProviderService _customerInfrastructureProviderService;
		private readonly ICustomerQueryInfrastructureProviderService _customerQueryInfrastructureProviderService;
		private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
		private readonly ILogger<CustomerUpdateUseCase> _logger;

		public CustomerUpdateUseCase(
			ExampleScopedSettings scopedSettings,
			IValidationRuleEngine validationConfiguration,
			ICustomerInfrastructureProviderService customerInfrastructureProviderService,
			ICustomerQueryInfrastructureProviderService customerQueryInfrastructureProviderService,
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService,
			ILogger<CustomerUpdateUseCase> logger
		) : base(validationConfiguration)
		{
			_scopedSettings = scopedSettings;
			_customerInfrastructureProviderService = customerInfrastructureProviderService;
			_customerQueryInfrastructureProviderService = customerQueryInfrastructureProviderService;
			_officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerUpdateResponse>> UpdateAsync(CustomerUpdateRequest request)
		{
			try
			{
				var customerResult = await _customerInfrastructureProviderService.ReadAsync(request.CustomerId);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerUpdateResponse>();
				}

				if (customerResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<CustomerUpdateResponse>();
				}

				var patchesResult = request.Customer.GetPatchInformation(_customerMappings);
				if (patchesResult.IsFaulty)
				{
					return patchesResult.ConvertTo<CustomerUpdateResponse>();
				}

				patchesResult = patchesResult.Data.CheckAndConvertValueObjectPatches(customerResult.Data);
				if (patchesResult.IsFaulty)
				{
					return patchesResult.ConvertTo<CustomerUpdateResponse>();
				}

				var constraintsResult = await CheckValidationConstraintsAsync(customerResult.Data, patchesResult.Data);
				if (constraintsResult.IsFaulty)
				{
					return constraintsResult.ConvertTo<CustomerUpdateResponse>();
				}

				var entityPatchResult = customerResult.Data.Patch(patchesResult.Data);
				if (entityPatchResult.IsFaulty)
				{
					return entityPatchResult.ConvertTo<CustomerUpdateResponse>();
				}

				var processOfficesChangesResult = await ProcessOfficesChangesAsync(customerResult.Data, request.Customer);
				if (processOfficesChangesResult.IsFaulty)
				{
					return processOfficesChangesResult.ConvertTo<CustomerUpdateResponse>();
				}

				if (!customerResult.Data.IsChanged)
				{
					return new CustomerUpdateResponse().ToResponseData(HttpStatusCode.NoContent);
				}

				var saveResult = await _customerInfrastructureProviderService.SaveAsync(customerResult.Data);
				if (saveResult.IsFaulty)
				{
					return saveResult.ConvertTo<CustomerUpdateResponse>();
				}

				return new CustomerUpdateResponse()
					.ToResponseData(HttpStatusCode.NoContent);
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Customer could not be updated", ex, additional: new
				{
					request.Customer,
					request.CustomerId
				});

				return ResponseData<CustomerUpdateResponse>.CreateInternalServerError(MessageConstants.UPDATEDATAERROR, ex, messageGuid);
			}
		}

		public ResponseData<ValidationConfigurationResponse> GetValidationConfiguration()
		{
			return GetValidationConfiguration<CustomerUpdateDto>(true);
		}

		private async Task<ResponseData<bool>> ProcessOfficesChangesAsync(Customer customer, PartialPutDocument<CustomerUpdateDto> customerDocument)
		{
			var changesResult = customerDocument.GetPatchInformation<CustomerUpdateDto, CustomerUpdateOfficeDto, Office, int>(p => p.Offices, _officeMappings);
			if (changesResult.IsFaulty)
			{
				return changesResult.ConvertTo<bool>();
			}

			var createResult = await CreateOfficesAsync(customer, changesResult.Data.ItemsToAdd);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<bool>();
			}

			foreach (var office in changesResult.Data.ItemsToPatch)
			{
				var updateResult = await UpdateOfficeAsync(customer, office);
				if (updateResult.IsFaulty)
				{
					return updateResult.ConvertTo<bool>();
				}
			}

			foreach (var office in changesResult.Data.ItemsToRemove)
			{
				var deactivateResult = DeactivateOffice(customer, office);
				if (deactivateResult.IsFaulty)
				{
					return deactivateResult;
				}
			}

			return true.ToResponseData();
		}

		private async Task<ResponseData<bool>> CreateOfficesAsync(Customer customer, IEnumerable<CustomerUpdateOfficeDto> offices)
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

		private async Task<ResponseData<Office>> CreateOfficeAsync(Customer customer, CustomerUpdateOfficeDto office)
		{
			var constraintsResult = await CheckValidationConstraintsAsync(office, customer.Id.Value);
			if (constraintsResult.IsFaulty)
			{
				return constraintsResult.ConvertTo<Office>();
			}

			return customer.AddOffice(office);
		}

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(CustomerUpdateOfficeDto office, int customerId)
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

		private async Task<ResponseData<Office>> UpdateOfficeAsync(Customer customer, KeyValuePair<int, IList<Patch<Office>>> officePatches)
		{
			var officeResult = customer.GetOffice(officePatches.Key);
			if (officeResult.IsFaulty)
			{
				return officeResult;
			}

			var constraintsResult = await CheckValidationConstraintsAsync(officeResult.Data, officePatches.Value, customer.Id.Value);
			if (constraintsResult.IsFaulty)
			{
				return constraintsResult.ConvertTo<Office>();
			}

			var patchesResult = officePatches.Value.CheckAndConvertValueObjectPatches(officeResult.Data);
			if (patchesResult.IsFaulty)
			{
				return patchesResult.ConvertTo<Office>();
			}

			var officePatchResult = officeResult.Data.Patch(patchesResult.Data);
			if (officePatchResult.IsFaulty)
			{
				return officePatchResult.ConvertTo<Office>();
			}

			return officeResult;
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

		private async Task<ResponseData<bool>> CheckValidationConstraintsAsync(Customer customer, IList<Patch<Customer>> patches)
		{
			var namePatch = patches.FirstOrDefault(p => p.PropertyName == nameof(Customer.Name));
			if (namePatch?.Value is not null)
			{
				var nameValue = (string)namePatch.Value;
				var isUniqueNameResult = await _customerQueryInfrastructureProviderService.IsUniqueNameAsync(customer.Id, nameValue);
				if (isUniqueNameResult.IsFaulty)
				{
					return isUniqueNameResult;
				}

				if (!isUniqueNameResult.Data)
				{
					return ResponseData<bool>.CreateInvalidDataResponse()
						.AddValidationError(nameof(Customer.Name), "Unique", nameValue);
				}
			}

			return true.ToResponseData();
		}
	}
}