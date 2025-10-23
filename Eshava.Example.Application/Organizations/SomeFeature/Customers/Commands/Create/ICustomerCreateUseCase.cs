using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create
{
	public interface ICustomerCreateUseCase
	{
		Task<ResponseData<CustomerCreateResponse>> CreateAsync(CustomerCreateRequest request);
		ResponseData<ValidationConfigurationResponse> GetValidationConfiguration();
	}
}