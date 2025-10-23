using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Classification Classification { get; set; }
	}
}