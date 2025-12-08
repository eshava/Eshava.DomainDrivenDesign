using Eshava.Core.Linq;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations
{
	internal class OrganizationsTransformProfile: TransformProfile
	{
		public OrganizationsTransformProfile()
		{
			CreateMap<CustomerSearchDto, Customers.Customer>()
				.ForPath(s => s.Street, t => t.AddressStreet)
				.ForPath(s => s.StreetNumber, t => t.AddressStreetNumber)
				.ForPath(s => s.City, t => t.AddressCity)
				.ForPath(s => s.ZipCode, t => t.AddressZipCode)
				.ForPath(s => s.Country, t => t.AddressCountry)
				;

			CreateMap<OfficeSearchDto, Offices.Office>()
				.ForPath(s => s.CustomerName, t => t.Customer.CompanyName)
				.ForPath(s => s.CustomerClassification, t => t.Customer.Classification)
				.ForPath(s => s.CustomerAddress.Street, t => t.Customer.AddressStreet)
				.ForPath(s => s.CustomerAddress.StreetNumber, t => t.Customer.AddressStreetNumber)
				.ForPath(s => s.CustomerAddress.City, t => t.Customer.AddressCity)
				.ForPath(s => s.CustomerAddress.ZipCode, t => t.Customer.AddressZipCode)
				.ForPath(s => s.CustomerAddress.Country, t => t.Customer.AddressCountry)
				.ForPath(s => s.OfficeAddress.Street, t => t.AddressStreet)
				.ForPath(s => s.OfficeAddress.StreetNumber, t => t.AddressStreetNumber)
				.ForPath(s => s.OfficeAddress.City, t => t.AddressCity)
				.ForPath(s => s.OfficeAddress.ZipCode, t => t.AddressZipCode)
				.ForPath(s => s.OfficeAddress.Country, t => t.AddressCountry)
				;
		}
	}
}