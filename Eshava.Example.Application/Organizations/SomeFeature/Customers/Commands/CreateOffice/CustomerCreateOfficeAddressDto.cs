using System.ComponentModel.DataAnnotations;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice
{
	public class CustomerCreateOfficeAddressDto
	{
		[Required]
		[MaxLength(50)]
		public string Street { get; set; }

		[Required]
		[MaxLength(10)]
		public string StreetNumber { get; set; }

		[Required]
		[MaxLength(50)]
		public string City { get; set; }

		[Required]
		[MaxLength(20)]
		public string ZipCode { get; set; }

		[Required]
		[MaxLength(50)]
		public string Country { get; set; }
	}
}