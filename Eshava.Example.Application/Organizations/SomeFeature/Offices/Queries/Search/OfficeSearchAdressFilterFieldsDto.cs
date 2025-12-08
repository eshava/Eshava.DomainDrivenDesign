using Eshava.Core.Linq.Attributes;
using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchAdressFilterFieldsDto : NestedFilter
	{
		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		[AllowedCompareOperator(CompareOperator.StartsWith)]
		public FilterField Street { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		[AllowedCompareOperator(CompareOperator.StartsWith)]
		public FilterField StreetNumber { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		[AllowedCompareOperator(CompareOperator.StartsWith)]
		public FilterField City { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		[AllowedCompareOperator(CompareOperator.StartsWith)]
		public FilterField ZipCode { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		[AllowedCompareOperator(CompareOperator.StartsWith)]
		public FilterField Country { get; set; }
	}
}