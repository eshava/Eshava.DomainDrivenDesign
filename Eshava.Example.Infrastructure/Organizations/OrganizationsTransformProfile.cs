using Eshava.Core.Linq;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations
{
	internal class OrganizationsTransformProfile: TransformProfile
	{
		public OrganizationsTransformProfile()
		{
			CreateMap<OfficeSearchDto, Offices.Office>()
				.ForPath(s => s.CustomerName, t => t.Customer.CompanyName)
				.ForPath(s => s.CustomerClassification, t => t.Customer.Classification)
				;
		}
	}
}