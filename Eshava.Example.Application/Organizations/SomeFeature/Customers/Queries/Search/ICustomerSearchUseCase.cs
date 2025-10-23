using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public interface ICustomerSearchUseCase
	{
		Task<ResponseData<CustomerSearchResponse>> SearchAsync(CustomerSearchRequest request);
	}
}