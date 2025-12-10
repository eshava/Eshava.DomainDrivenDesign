using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchAddressSortFieldsDto : NestedSort
	{
		public SortField Street { get; set; }
		public SortField StreetNumber { get; set; }
		public SortField City { get; set; }
		public SortField ZipCode { get; set; }
		public SortField Country { get; set; }
	}
}