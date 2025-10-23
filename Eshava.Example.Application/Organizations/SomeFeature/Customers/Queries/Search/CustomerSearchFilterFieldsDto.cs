using Eshava.Core.Linq.Attributes;
using Eshava.Core.Linq.Enums;
using Eshava.Core.Linq.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchFilterFieldsDto
	{
		[AllowedCompareOperator(CompareOperator.Equal)]
		[AllowedCompareOperator(CompareOperator.Contains)]
		public FilterField Name { get; set; }
		
		[AllowedCompareOperator(CompareOperator.Equal)]
		public FilterField Classification { get; set; }
	}
}