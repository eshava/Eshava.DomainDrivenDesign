using System;
using System.Linq;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	internal class CustomerSearchUseCase : AbstractSearchUseCase<CustomerSearchRequest, CustomerSearchDto>, ICustomerSearchUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly ICustomerQueryInfrastructureProviderService _customerQueryInfrastructureProviderService;
		private readonly ILogger<CustomerSearchUseCase> _logger;

		public CustomerSearchUseCase(
			ExampleScopedSettings scopedSettings,
			IWhereQueryEngine whereQueryEngine,
			ISortingQueryEngine sortingQueryEngine,
			ICustomerQueryInfrastructureProviderService customerQueryInfrastructureProviderService,
			ILogger<CustomerSearchUseCase> logger
		) : base(whereQueryEngine, sortingQueryEngine)
		{
			_scopedSettings = scopedSettings;
			_customerQueryInfrastructureProviderService = customerQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<CustomerSearchResponse>> SearchAsync(CustomerSearchRequest request)
		{
			try
			{
				if (request.Filter == null)
				{
					request.Filter = new CustomerSearchFilterDto();
				}

				var filterRequestResult = GetFilterRequest(request.Filter);
				if (filterRequestResult.IsFaulty)
				{
					return filterRequestResult.ConvertTo<CustomerSearchResponse>();
				}

				var filterRequest = filterRequestResult.Data;
				var customerResult = await _customerQueryInfrastructureProviderService.SearchAsync(filterRequest);
				if (customerResult.IsFaulty)
				{
					return customerResult.ConvertTo<CustomerSearchResponse>();
				}

				var dtos = customerResult.Data.ToList();
				var searchResult = new CustomerSearchResponse()
				{
					Customers = dtos,
					Total = 0
				};
				if (request.Filter.Skip == 0 && request.Filter.Take > 0 && dtos.Count == request.Filter.Take)
				{
					filterRequest.Skip = 0;
					filterRequest.Take = 0;
					var customerCountResult = await _customerQueryInfrastructureProviderService.SearchCountAsync(filterRequest);
					if (customerCountResult.IsFaulty)
					{
						return customerCountResult.ConvertTo<CustomerSearchResponse>();
					}

					searchResult.Total = customerCountResult.Data;
				}
				else if (request.Filter.Skip == 0)
				{
					searchResult.Total = searchResult.Customers.Count();
				}

				var adjustDtosResult = await AdjustDtosAsync(request, searchResult.Customers);
				if (adjustDtosResult.IsFaulty)
				{
					return adjustDtosResult.ConvertTo<CustomerSearchResponse>();
				}

				searchResult.Customers = adjustDtosResult.Data;

				return searchResult.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Customer could not be read", ex);

				return ResponseData<CustomerSearchResponse>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}
	}
}