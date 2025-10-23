using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.DomainDrivenDesign.Domain.Attributes;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class ObjectExtensions
	{
		public static IList<Patch<TDomain>> ToPatches<TDto, TDomain>(this TDto dto, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<TDomain, object>> Domain)> mappings = null)
			where TDto : class
			where TDomain : class
		{
			var patches = new List<Patch<TDomain>>();
			var dtoType = typeof(TDto);
			var domainType = typeof(TDomain);

			var dtoPropertyInfos = dtoType.GetProperties();
			var domainPropertyInfos = domainType.GetProperties();

			foreach (var dtoPropertyInfo in dtoPropertyInfos)
			{
				var dtoValue = dtoPropertyInfo.GetValue(dto, null);

				if (mappings is not null)
				{
					var mapping = mappings.FirstOrDefault(m => m.Dto.GetMemberExpressionString() == dtoPropertyInfo.Name);
					if (mapping.Dto is not null)
					{
						patches.Add(Patch<TDomain>.Create(mapping.Domain, dtoValue));

						continue;
					}
				}

				var domainPropertyInfo = domainPropertyInfos.FirstOrDefault(p => p.Name == dtoPropertyInfo.Name);
				if (domainPropertyInfo is null || !domainPropertyInfo.CanWrite)
				{
					continue;
				}

				var autoPatchBlocked = domainPropertyInfo.GetCustomAttribute<AutoPatchBlockedAttribute>();
				if (autoPatchBlocked is not null)
				{
					continue;
				}

				patches.Add(domainPropertyInfo.ToPatch<TDomain, object>(dtoValue));
			}

			return patches;
		}
	}
}