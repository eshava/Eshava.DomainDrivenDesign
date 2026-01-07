using Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal interface IOfficeRepository : IAbstractChildDomainModelRepository<Domain.Organizations.SomeFeature.Office, Customers.CustomerCreationBag, int>
	{

	}
}