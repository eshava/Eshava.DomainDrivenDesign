namespace Eshava.Test.DomainDrivenDesign.Domain.Extensions.Models
{
	/// <summary>
	/// Carries only part of the address, so the value object has to be completed with default values
	/// </summary>
	public class AddressDto
	{
		public string Street { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
	}
}