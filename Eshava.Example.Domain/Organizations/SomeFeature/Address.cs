using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.Example.Domain.Organizations.SomeFeature
{
	/// <summary>
	/// Value Object
	/// </summary>
	public class Address : AbstractValueObject
	{
		public Address(string street, string streetNumber, string city, string zipCode, string country)
		{
			Street = street?.Trim();
			StreetNumber = streetNumber?.Trim();
			City = city?.Trim();
			ZipCode = zipCode?.Trim();
			Country = country?.Trim();
		}

		[Required]
		[MaxLength(50)]
		public string Street { get; }

		[Required]
		[MaxLength(10)]
		public string StreetNumber { get; }

		[Required]
		[MaxLength(50)]
		public string City { get; }

		[Required]
		[MaxLength(20)]
		public string ZipCode { get; }

		[Required]
		[MaxLength(50)]
		public string Country { get; }

		public override IEnumerable<ValidationError> Validate()
		{
			if (!StreetNumber.IsNullOrEmpty())
			{
				var streetNumberParts = StreetNumber.ToCharArray();
				var isValid = streetNumberParts.All(c => System.Char.IsDigit(c) || c == '-' || c == ' ');
				if (!isValid)
				{
					return [new ValidationError
					{
						PropertyName = nameof(StreetNumber),
						ErrorType = "InvalidFormat",
						Value = StreetNumber
					}];
				}
			}

			return base.Validate();
		}
	}
}