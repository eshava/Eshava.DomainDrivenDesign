using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal interface IOfficeQueryRepository
	{
		Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, int? officeId, string name);
		Task<ResponseData<bool>> ExistsAsync(int officeId);
		Task<ResponseData<OfficeReadDto>> ReadAsync(int officeId);
		Task<ResponseData<IEnumerable<OfficeSearchDto>>> SearchAsync(FilterRequestDto<OfficeSearchDto> searchRequest);
		Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<OfficeSearchDto> searchRequest);
	}
}