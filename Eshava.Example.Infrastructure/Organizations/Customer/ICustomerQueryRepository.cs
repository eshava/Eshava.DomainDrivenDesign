using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal interface ICustomerQueryRepository
	{
		Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, string name);
		Task<ResponseData<CustomerReadDto>> ReadAsync(int customerId);
		Task<ResponseData<IEnumerable<CustomerSearchDto>>> SearchAsync(FilterRequestDto<CustomerSearchDto> searchRequest);
		Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<CustomerSearchDto> searchRequest);
	}
}