using System.ComponentModel.DataAnnotations;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice
{
	public class CustomerUpdateOfficeDto
	{
		[Required]
		[MaxLength(250)]
		public string Name { get; set; }
	}
}