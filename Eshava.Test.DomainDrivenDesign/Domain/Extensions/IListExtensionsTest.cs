using System.Collections.Generic;
using System.Linq;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.Test.DomainDrivenDesign.Domain.Extensions.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.DomainDrivenDesign.Domain.Extensions
{
	[TestClass, TestCategory("Domain")]
	public class IListExtensionsTest
	{
		[TestMethod]
		public void CheckAndConvertValueObjectPatchesWithoutValueObjectLeavesMissingReferenceTypeParameterNullTest()
		{
			// Arrange
			var company = new Company { Name = "Company" };
			var patches = new List<Patch<Company>>
			{
				Patch<Company>.Create(p => p.Address.Street, "Main Street"),
				Patch<Company>.Create(p => p.Address.ZipCode, "12345"),
				Patch<Company>.Create(p => p.Address.City, "Springfield"),
				Patch<Company>.Create(p => p.Address.CountryId, 1)
			};

			// Act
			var result = patches.CheckAndConvertValueObjectPatches(company);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);
			result.Data.Single().PropertyName.Should().Be(nameof(Company.Address));

			var address = result.Data.Single().Value as Address;
			address.Should().NotBeNull();
			address.Street.Should().Be("Main Street");
			address.HouseNumber.Should().BeNull();
			address.ZipCode.Should().Be("12345");
			address.City.Should().Be("Springfield");
			address.CountryId.Should().Be(1);
		}

		[TestMethod]
		public void CheckAndConvertValueObjectPatchesWithoutValueObjectLeavesMissingValueTypeParameterDefaultTest()
		{
			// Arrange
			var company = new Company { Name = "Company" };
			var patches = new List<Patch<Company>>
			{
				Patch<Company>.Create(p => p.Address.Street, "Main Street"),
				Patch<Company>.Create(p => p.Address.HouseNumber, "1")
			};

			// Act
			var result = patches.CheckAndConvertValueObjectPatches(company);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(1);

			var address = result.Data.Single().Value as Address;
			address.Should().NotBeNull();
			address.Street.Should().Be("Main Street");
			address.HouseNumber.Should().Be("1");
			address.ZipCode.Should().BeNull();
			address.City.Should().BeNull();
			address.CountryId.Should().Be(0);
		}

		[TestMethod]
		public void CheckAndConvertValueObjectPatchesWithValueObjectTakesMissingParametersFromItTest()
		{
			// Arrange
			var company = new Company
			{
				Name = "Company",
				Address = new Address("Main Street", "1", "12345", "Springfield", 1)
			};
			var patches = new List<Patch<Company>>
			{
				Patch<Company>.Create(p => p.Name, "Renamed"),
				Patch<Company>.Create(p => p.Address.Street, "Side Street")
			};

			// Act
			var result = patches.CheckAndConvertValueObjectPatches(company);

			// Assert
			result.IsFaulty.Should().BeFalse();
			result.Data.Should().HaveCount(2);
			result.Data.Should().Contain(p => p.PropertyName == nameof(Company.Name) && Equals(p.Value, "Renamed"));

			var address = result.Data.Single(p => p.PropertyName == nameof(Company.Address)).Value as Address;
			address.Should().NotBeNull();
			address.Street.Should().Be("Side Street");
			address.HouseNumber.Should().Be("1");
			address.ZipCode.Should().Be("12345");
			address.City.Should().Be("Springfield");
			address.CountryId.Should().Be(1);
		}
	}
}