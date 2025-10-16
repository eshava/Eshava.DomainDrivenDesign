using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.PartialPut;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Eshava.DomainDrivenDesign.Application.Extensions
{
	public static class PartialPutDocumentExtensions
	{
		public static ResponseData<IList<Patch<Target>>> GetPatchInformation<Source, Target>(this PartialPutDocument<Source> document, IEnumerable<(Expression<Func<Source, object>> Source, Expression<Func<Target, object>> Target)> mappings = null)
			where Source : class
			where Target : class
		{
			return (document as PartialPutDocumentLayer).GetPatchInformation<Source, Target>(mappings);
		}

		public static ResponseData<EnumerablePatchDataDto<TIdentifier, TargetDto, TargetDomain>> GetPatchInformation<Source, TargetDto, TargetDomain, TIdentifier>(
			this PartialPutDocument<Source> document,
			Expression<Func<Source, object>> enumerableProperty,
			IEnumerable<(Expression<Func<TargetDto, object>> Source, Expression<Func<TargetDomain, object>> Target)> mappings = null
		)
			where Source : class
			where TargetDto : class
			where TargetDomain : class
			where TIdentifier : struct
		{
			return ((PartialPutDocumentLayer)document).GetPatchInformation<Source, TargetDto, TargetDomain, TIdentifier>(enumerableProperty, mappings);
		}

		public static Patch<Target> GetSpecificPatchInformation<Source, Target>(this PartialPutDocument<Source> document, Expression<Func<Source, object>> source, Expression<Func<Target, object>> target = null)
			where Source : class
			where Target : class
		{
			var sourcePropertyName = source.GetMemberExpressionString();

			var operation = document.Operations.FirstOrDefault(p => p.Type == PartialPutOperationType.Replace && p.PropertyName == sourcePropertyName);
			if (operation == null)
			{
				return null;
			}

			if (target is not null)
			{
				return Patch<Target>.Create(target, operation.Value);
			}

			var patchResult = sourcePropertyName.ToPatch<Target, object>(operation.Value);

			return patchResult.IsFaulty
				? null
				: patchResult.Data;
		}
	}
}