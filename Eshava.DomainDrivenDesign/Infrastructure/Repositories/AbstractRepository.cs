using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Linq.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.DomainDrivenDesign.Infrastructure.Repositories
{
	public abstract class AbstractRepository
    {
        public AbstractRepository(
            ITransformQueryEngine transformQueryEngine
        )
        {
            TransformQueryEngine = transformQueryEngine;
        }

        protected ITransformQueryEngine TransformQueryEngine { get; }

        protected FilterRequestDto<TData> TransformFilterRequest<TDto, TData>(FilterRequestDto<TDto> sourceFilterRequest)
            where TData : class
            where TDto : class
        {
            var targetFilterRequest = new FilterRequestDto<TData>
            {
                Skip = sourceFilterRequest.Skip,
                Take = sourceFilterRequest.Take,
                Where = [],
                Sort = []
            };

            if ((sourceFilterRequest.Where?.Count ?? 0) > 0)
            {
                foreach (var whereCondition in sourceFilterRequest.Where)
                {
                    targetFilterRequest.Where.Add(TransformQueryEngine.Transform<TDto, TData>(whereCondition));
                }
            }

            if ((sourceFilterRequest.Sort?.Count ?? 0) > 0)
            {
                foreach (var sortCondition in sourceFilterRequest.Sort)
                {
                    var expression = TransformQueryEngine.TransformMemberExpression<TDto, TData>(sortCondition.Member);

                    targetFilterRequest.Sort.Add(new OrderByCondition
                    {
                        Member = expression.Member,
                        Parameter = expression.Parameter,
                        SortOrder = sortCondition.SortOrder
                    });
                }
            }

            return targetFilterRequest;
        }
    }
}