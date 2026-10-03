using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.Test.DomainDrivenDesign.Domain.Extensions.Models
{
	public class Address : AbstractValueObject
	{
		public Address(string street, string houseNumber, string zipCode, string city, int countryId)
		{
			Street = street;
			HouseNumber = houseNumber;
			ZipCode = zipCode;
			City = city;
			CountryId = countryId;
		}

		public string Street { get; }
		public string HouseNumber { get; }
		public string ZipCode { get; }
		public string City { get; }
		public int CountryId { get; }
	}
}