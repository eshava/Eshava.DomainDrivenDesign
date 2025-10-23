using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Deactivate
{
	public interface ICustomerDeactivateUseCase
	{
		Task<ResponseData<CustomerDeactivateResponse>> DeactivateAsync(CustomerDeactivateRequest request);
	}
}