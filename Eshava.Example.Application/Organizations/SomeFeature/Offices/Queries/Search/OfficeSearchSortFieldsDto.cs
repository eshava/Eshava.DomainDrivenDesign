using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchSortFieldsDto
	{
		public SortField Name { get; set; }
		public SortField CustomerName { get; set; }
		public SortField CustomerClassification { get; set; }

		public OfficeSearchAddressSortFieldsDto CustomerAddress { get; set; }
		public OfficeSearchAddressSortFieldsDto OfficeAddress { get; set; }
	}
}