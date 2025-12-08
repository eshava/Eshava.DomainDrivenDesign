using System.ComponentModel.DataAnnotations;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create
{
	public class CustomerCreateOfficeDto
	{
		[Required]
		[MaxLength(250)]
		public string Name { get; set; }
		public CustomerCreateAddressDto Address { get; set; }	
	}
}