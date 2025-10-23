using System.Collections.Generic;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchResponse
	{
		public IEnumerable<OfficeSearchDto> Offices { get; set; }
		public int Total { get; set; }
	}
}