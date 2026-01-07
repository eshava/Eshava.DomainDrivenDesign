using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerQueryInfrastructureProviderService : ICustomerQueryInfrastructureProviderService
	{
		private readonly ICustomerQueryRepository _customerQueryRepository;

		public CustomerQueryInfrastructureProviderService(ICustomerQueryRepository customerQueryRepository)
		{
			_customerQueryRepository = customerQueryRepository;
		}

		public Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, string name)
		{
			return _customerQueryRepository.IsUniqueNameAsync(customerId, name);
		}

		public Task<ResponseData<CustomerReadDto>> ReadAsync(int customerId)
		{
			return _customerQueryRepository.ReadAsync(customerId);
		}

		public Task<ResponseData<IEnumerable<CustomerSearchDto>>> SearchAsync(FilterRequestDto<CustomerSearchDto> searchRequest)
		{
			return _customerQueryRepository.SearchAsync(searchRequest);
		}

		public Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<CustomerSearchDto> searchRequest)
		{
			return _customerQueryRepository.SearchCountAsync(searchRequest);
		}
	}
}