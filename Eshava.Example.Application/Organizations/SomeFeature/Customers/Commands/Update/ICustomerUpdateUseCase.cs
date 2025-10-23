using System.Threading.Tasks;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update
{
	public interface ICustomerUpdateUseCase
	{
		Task<ResponseData<CustomerUpdateResponse>> UpdateAsync(CustomerUpdateRequest request);
		ResponseData<ValidationConfigurationResponse> GetValidationConfiguration();
	}
}