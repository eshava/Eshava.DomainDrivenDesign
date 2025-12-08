using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchSortFieldsDto
	{
		public SortField Name { get; set; }
		public SortField Classification { get; set; }
		public SortField Street { get; set; }
		public SortField StreetNumber { get; set; }
		public SortField City { get; set; }
		public SortField ZipCode { get; set; }
		public SortField Country { get; set; }

	}
}