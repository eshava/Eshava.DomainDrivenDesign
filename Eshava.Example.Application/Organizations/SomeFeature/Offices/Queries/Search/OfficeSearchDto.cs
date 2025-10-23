using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchDto
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public string CustomerName { get; set; }
		public Classification CustomerClassification { get; set; }
		public string Name { get; set; }
	}
}