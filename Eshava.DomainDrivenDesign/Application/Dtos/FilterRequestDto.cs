using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eshava.Core.Linq.Models;

namespace Eshava.DomainDrivenDesign.Application.Dtos
{
	public class FilterRequestDto<T> where T : class
	{
		public FilterRequestDto()
		{
			Where = [];
			Sort = [];
		}

		public int Skip { get; set; }
		public int Take { get; set; }
		public List<Expression<Func<T, bool>>> Where { get; set; }
		public List<OrderByCondition> Sort { get; set; }

		public static FilterRequestDto<T> Create(params Expression<Func<T, bool>>[] expression)
		{
			return new FilterRequestDto<T>
			{
				Where = expression.Where(e => e is not null).ToList()
			};
		}
	}
}