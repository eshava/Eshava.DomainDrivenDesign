using System.ComponentModel.DataAnnotations;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update
{
	public class CustomerUpdateOfficeDto
	{
		public int Id { get; set; }

		[Required]
		[MaxLength(250)]
		public string Name { get; set; }

		public CustomerUpdateAddressDto Address { get; set; }	
	}
}