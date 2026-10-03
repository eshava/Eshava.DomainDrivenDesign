using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Test.DomainDrivenDesign.Domain.Extensions.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.DomainDrivenDesign.Domain.Extensions
{
	[TestClass, TestCategory("Domain")]
	public class ObjectExtensionsTest
	{
		[TestMethod]
		public void ToPatchesWithIncompleteValueObjectDtoUsesDefaultValuesForMissingParametersTest()
		{
			// Arrange
			var dto = new CompanyDto
			{
				Name = "Company",
				Address = new AddressDto
				{
					Street = "Main Street",
					ZipCode = "12345",
					City = "Springfield"
				}
			};
			var mappings = new List<(Expression<Func<CompanyDto, object>> Dto, Expression<Func<Company, object>> Domain)>
			{
				(p => p.Address.Street, p => p.Address.Street),
				(p => p.Address.ZipCode, p => p.Address.ZipCode),
				(p => p.Address.City, p => p.Address.City)
			};

			// Act
			var patches = dto.ToPatches(mappings);

			// Assert
			patches.Should().HaveCount(2);
			patches.Should().Contain(p => p.PropertyName == nameof(Company.Name) && Equals(p.Value, "Company"));

			var address = patches.Single(p => p.PropertyName == nameof(Company.Address)).Value as Address;
			address.Should().NotBeNull();
			address.Street.Should().Be("Main Street");
			address.HouseNumber.Should().BeNull();
			address.ZipCode.Should().Be("12345");
			address.City.Should().Be("Springfield");
			address.CountryId.Should().Be(0);
		}
	}
}