using System.Collections.Generic;
using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	public class CustomerReadDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Classification Classification { get; set; }

		public IEnumerable<CustomerReadOfficeDto> Offices { get; set; }
	}
}