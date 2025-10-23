using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice
{
	public interface ICustomerUpdateOfficeUseCase
	{
		Task<ResponseData<CustomerUpdateOfficeResponse>> UpdateOfficeAsync(CustomerUpdateOfficeRequest request);
		ResponseData<ValidationConfigurationResponse> GetValidationConfiguration();
	}
}