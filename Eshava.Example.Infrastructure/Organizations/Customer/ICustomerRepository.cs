using Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal interface ICustomerRepository : IAbstractDomainModelRepository<Domain.Organizations.SomeFeature.Customer, int>
	{

	}
}