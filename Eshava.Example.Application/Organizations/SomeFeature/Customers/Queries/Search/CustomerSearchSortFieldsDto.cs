using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchSortFieldsDto
	{
		public SortField Name { get; set; }
		public SortField Classification { get; set; }
	}
}