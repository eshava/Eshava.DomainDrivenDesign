using Eshava.DomainDrivenDesign.Application.Interfaces.Providers;
using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands
{
	public interface ICustomerInfrastructureProviderService : IInfrastructureProvider<Customer, int>
	{

	}
}