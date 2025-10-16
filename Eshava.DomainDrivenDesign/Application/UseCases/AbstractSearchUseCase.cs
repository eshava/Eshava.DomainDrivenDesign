using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Domain.Extensions;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public abstract class AbstractSearchUseCase<TRequest, TDto>
		where TRequest : class
		where TDto : class
	{
		private readonly IWhereQueryEngine _whereQueryEngine;
		private readonly ISortingQueryEngine _sortingQueryEngine;

		public AbstractSearchUseCase(
			 IWhereQueryEngine whereQueryEngine,
			 ISortingQueryEngine sortingQueryEngine
		)
		{
			_whereQueryEngine = whereQueryEngine;
			_sortingQueryEngine = sortingQueryEngine;
		}

		protected ResponseData<FilterRequestDto<TDto>> GetFilterRequest(AbstractFilterDto filterDto, Dictionary<string, List<Expression<Func<TDto, object>>>> mappings = null, bool caseInsensitive = false)
		{
			return GetFilterRequest<TDto>(filterDto, mappings, caseInsensitive);
		}

		protected ResponseData<FilterRequestDto<Target>> GetFilterRequest<Target>(AbstractFilterDto filterDto, Dictionary<string, List<Expression<Func<Target, object>>>> mappings = null, bool caseInsensitive = false)
			where Target : class
		{
			var sortFields = filterDto?.GetSortFields();
			var filterfields = filterDto?.GetFilterFields();
			var searchTerm = filterDto?.GetSearchTerm() ?? "";
			var options = new Core.Linq.Models.WhereQueryEngineOptions { CaseInsensitive = caseInsensitive };

			var whereQueryResult = filterfields != null || !searchTerm.IsNullOrEmpty()
				? _whereQueryEngine.BuildQueryExpressions(filterfields ?? new object(), filterDto.GetSearchTerm(), mappings, options)
				: new List<Expression<Func<Target, bool>>>().ToIEnumerableResponseData()
				;

			if (whereQueryResult.IsFaulty)
			{
				return whereQueryResult.ConvertTo<FilterRequestDto<Target>>();
			}

			var sortQueryResult = sortFields != null
				? _sortingQueryEngine.BuildSortConditions(sortFields, mappings)
				: new List<Core.Linq.Models.OrderByCondition>().ToIEnumerableResponseData()
				;

			if (sortQueryResult.IsFaulty)
			{
				return sortQueryResult.ConvertTo<FilterRequestDto<Target>>();
			}

			return new ResponseData<FilterRequestDto<Target>>
			{
				Data = new FilterRequestDto<Target>
				{
					Take = filterDto?.Take ?? 0,
					Skip = filterDto?.Skip ?? 0,
					Where = whereQueryResult.Data.ToList(),
					Sort = sortQueryResult.Data.ToList()
				}
			};
		}

		protected virtual Task<ResponseData<IEnumerable<TDto>>> AdjustDtosAsync(TRequest request, IEnumerable<TDto> dtos)
		{
			return dtos?.ToResponseDataAsync();
		}
	}
}