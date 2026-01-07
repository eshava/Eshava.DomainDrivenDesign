using System;
using System.Collections.Generic;
using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	public class CustomerReadDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Classification Classification { get; set; }

		public string Street { get; set; }
		public string StreetNumber { get; set; }
		public string City { get; set; }
		public string ZipCode { get; set; }
		public string Country { get; set; }
		public int Version { get; set; }
		public IEnumerable<DateTime> VersionChangedAtUtc { get; set; }
		public IEnumerable<CustomerReadOfficeDto> Offices { get; set; }
	}
}