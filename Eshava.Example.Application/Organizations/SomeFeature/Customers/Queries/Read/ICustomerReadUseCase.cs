using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	public interface ICustomerReadUseCase
	{
		Task<ResponseData<CustomerReadResponse>> ReadAsync(CustomerReadRequest request);
	}
}