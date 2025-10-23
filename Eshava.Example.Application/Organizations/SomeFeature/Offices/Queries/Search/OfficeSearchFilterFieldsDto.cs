using Eshava.Core.Linq.Attributes;
using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchFilterFieldsDto
	{
		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		public FilterField Name { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		public FilterField CustomerName { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.NotEqual)]
		public FilterField CustomerId { get; set; }

		[AllowedCompareOperator(CompareOperator.Equal)]
		public FilterField CustomerClassification { get; set; }
	}
}