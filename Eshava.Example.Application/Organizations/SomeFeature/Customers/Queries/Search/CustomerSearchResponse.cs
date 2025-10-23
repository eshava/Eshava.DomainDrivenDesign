using System.Collections.Generic;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchResponse
	{
		public IEnumerable<CustomerSearchDto> Customers { get; set; }
		public int Total { get; set; }
	}
}