using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice
{
	public interface ICustomerCreateOfficeUseCase
	{
		Task<ResponseData<CustomerCreateOfficeResponse>> CreateOfficeAsync(CustomerCreateOfficeRequest request);
		ResponseData<ValidationConfigurationResponse> GetValidationConfiguration();
	}
}