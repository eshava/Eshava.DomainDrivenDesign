using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.DeactivateOffice
{
	public interface ICustomerDeactivateOfficeUseCase
	{
		Task<ResponseData<CustomerDeactivateOfficeResponse>> DeactivateOfficeAsync(CustomerDeactivateOfficeRequest request);
	}
}