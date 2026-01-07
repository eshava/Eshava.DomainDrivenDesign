using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class OfficeQueryInfrastructureProviderService : IOfficeQueryInfrastructureProviderService
	{
		private readonly IOfficeQueryRepository _officeQueryRepository;

		public OfficeQueryInfrastructureProviderService(IOfficeQueryRepository officeQueryRepository)
		{
			_officeQueryRepository = officeQueryRepository;
		}

		public Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, int? officeId, string name)
		{
			return _officeQueryRepository.IsUniqueNameAsync(customerId, officeId, name);
		}

		public Task<ResponseData<bool>> ExistsAsync(int officeId)
		{
			return _officeQueryRepository.ExistsAsync(officeId);
		}

		public Task<ResponseData<OfficeReadDto>> ReadAsync(int officeId)
		{
			return _officeQueryRepository.ReadAsync(officeId);
		}

		public Task<ResponseData<IEnumerable<OfficeSearchDto>>> SearchAsync(FilterRequestDto<OfficeSearchDto> searchRequest)
		{
			return _officeQueryRepository.SearchAsync(searchRequest);
		}

		public Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<OfficeSearchDto> searchRequest)
		{
			return _officeQueryRepository.SearchCountAsync(searchRequest);
		}
	}
}