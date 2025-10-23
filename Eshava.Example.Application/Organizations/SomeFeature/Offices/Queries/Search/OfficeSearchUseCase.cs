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

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	internal class OfficeSearchUseCase: AbstractSearchUseCase<OfficeSearchRequest, OfficeSearchDto>, IOfficeSearchUseCase
    {
        private readonly ExampleScopedSettings _scopedSettings;
        private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
        private readonly ILogger<OfficeSearchUseCase> _logger;
        
		public OfficeSearchUseCase(
			ExampleScopedSettings scopedSettings, 
			IWhereQueryEngine whereQueryEngine, 
			ISortingQueryEngine sortingQueryEngine, 
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService, 
			ILogger<OfficeSearchUseCase> logger
		) : base(whereQueryEngine, sortingQueryEngine)
        {
            _scopedSettings = scopedSettings;
            _officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
            _logger = logger;
        }

        public async Task<ResponseData<OfficeSearchResponse>> SearchAsync(OfficeSearchRequest request)
        {
            try
            {
                if (request.Filter == null)
                {
                    request.Filter = new OfficeSearchFilterDto();
                }

                var filterRequestResult = GetFilterRequest(request.Filter);
                if (filterRequestResult.IsFaulty)
                {
                    return filterRequestResult.ConvertTo<OfficeSearchResponse>();
                }

                var filterRequest = filterRequestResult.Data;
                var officeResult = await _officeQueryInfrastructureProviderService.SearchAsync(filterRequest);
                if (officeResult.IsFaulty)
                {
                    return officeResult.ConvertTo<OfficeSearchResponse>();
                }

                var dtos = officeResult.Data.ToList();
                var searchResult = new OfficeSearchResponse()
                {
                    Offices = dtos,
                    Total = 0
                };
                if (request.Filter.Skip == 0 && request.Filter.Take > 0 && dtos.Count == request.Filter.Take)
                {
                    filterRequest.Skip = 0;
                    filterRequest.Take = 0;
                    var officeCountResult = await _officeQueryInfrastructureProviderService.SearchCountAsync(filterRequest);
                    if (officeCountResult.IsFaulty)
                    {
                        return officeCountResult.ConvertTo<OfficeSearchResponse>();
                    }

                    searchResult.Total = officeCountResult.Data;
                }
                else if (request.Filter.Skip == 0)
                {
                    searchResult.Total = searchResult.Offices.Count();
                }

                var adjustDtosResult = await AdjustDtosAsync(request, searchResult.Offices);
                if (adjustDtosResult.IsFaulty)
                {
                    return adjustDtosResult.ConvertTo<OfficeSearchResponse>();
                }

                searchResult.Offices = adjustDtosResult.Data;

                return searchResult.ToResponseData();
            }
            catch (Exception ex)
            {
                var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Office could not be read", ex);

                return ResponseData<OfficeSearchResponse>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
            }
        }
    }
}