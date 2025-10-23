using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eshava.Example.Domain.Organizations.SomeFeature;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create
{
	public class CustomerCreateDto
	{
		[Required]
		[MaxLength(250)]
		public string Name { get; set; }
		public Classification Classification { get; set; }

		public IEnumerable<CustomerCreateOfficeDto> Offices { get; set; }
	}
}