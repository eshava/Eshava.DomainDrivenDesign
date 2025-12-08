using System.ComponentModel.DataAnnotations;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice
{
	public class CustomerCreateOfficeDto
	{
		[Required]
		[MaxLength(250)]
		public string Name { get; set; }

		public CustomerCreateOfficeAddressDto Address { get; set; }
	}
}