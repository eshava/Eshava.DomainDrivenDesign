using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Settings;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace Eshava.DomainDrivenDesign.Infrastructure.Repositories
{
	public abstract class AbstractDomainModelRepository<TDomain, TData, TIdentifier, TScopedSettings> : AbstractEntityRepository<TDomain, TData, TIdentifier, TScopedSettings>
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TData : AbstractDatabaseModel<TIdentifier>, new()
		where TIdentifier : struct
		where TScopedSettings : AbstractScopedSettings
	{
		private static readonly Type _typeValueObject = typeof(AbstractValueObject);

		public AbstractDomainModelRepository(
		   IDatabaseSettings databaseSettings,
		   TScopedSettings scopedSettings,
		   ITransformQueryEngine transformQueryEngine,
		   ILogger logger
		) : base(databaseSettings, scopedSettings, transformQueryEngine, logger)
		{

		}

		public virtual Task<ResponseData<TIdentifier>> CreateAsync(TDomain model)
		{
			try
			{
				var newEntity = FromDomainModel(model);

				return CreateAsync(newEntity);
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, $"Error creating {typeof(TData).Name}", ex);

				return ResponseData<TIdentifier>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex, messageGuid).ToTask();
			}
		}

		protected abstract TData FromDomainModel(TDomain model);

		protected virtual TData FromDomainModel(TData data, TDomain model)
		{
			return data;
		}

		protected IEnumerable<Patch<TDomain1>> GenerateDomainPatchList<TData1, TDomain1>(
			TData1 instance,
			IEnumerable<(Expression<Func<TData1, object>> DataProperty, Expression<Func<TDomain1, object>> DomainProperty)> propertyMappings = null,
			Dictionary<string, Func<object, object>> propertyValueToDomainMappings = null,
			Func<TData1, IValidationEngine, IEnumerable<Patch<TDomain1>>> createValueObjectPatches = null,
			IValidationEngine validationEngine = null
		)
			where TDomain1 : class
			where TData1 : AbstractDatabaseModel<TIdentifier>
		{
			var dataType = typeof(TData1);
			var domainType = typeof(TDomain1);
			var domainParameterExpression = Expression.Parameter(domainType, "p");
			var dataPropertyInfos = dataType.GetProperties();
			var domainPropertyInfos = domainType
				.GetProperties()
				.ToDictionary(p => p.Name, p => p);

			var patches = new List<Patch<TDomain1>>();

			var mappings = propertyMappings?.ToDictionary(
				m => m.DataProperty.GetMemberExpressionString(),
				m => domainPropertyInfos[m.DomainProperty.GetMemberExpressionString()]
			) ?? [];

			foreach (var propertyInfo in dataPropertyInfos)
			{
				var domainPropertyInfo = mappings.TryGetValue(propertyInfo.Name, out var propInfoMapping)
					? propInfoMapping
					: (domainPropertyInfos.TryGetValue(propertyInfo.Name, out var propInfo) ? propInfo : null);

				if (domainPropertyInfo is null)
				{
					continue;
				}

				var value = propertyInfo.GetValue(instance);
				var domainMemberExpression = Expression.MakeMemberAccess(domainParameterExpression, domainPropertyInfo);
				var domainValue = MapToDomainPropertyValue(domainPropertyInfo, propertyInfo.Name, value, propertyValueToDomainMappings);
				if (domainValue is null)
				{
					continue;
				}

				patches.Add(Patch<TDomain1>.Create(
					domainMemberExpression.ConvertToMemberExpressionFunction<TDomain1, object>(domainParameterExpression),
					domainValue
				));
			}

			if (createValueObjectPatches is not null)
			{
				var valueObjectPatches = createValueObjectPatches(instance, validationEngine);
				if (valueObjectPatches.Any())
				{
					patches.AddRange(valueObjectPatches);
				}
			}

			return patches;
		}

		protected object MapToDomainPropertyValue(PropertyInfo domainModelPropertyInfo, string dataPropertyName, object value, Dictionary<string, Func<object, object>> propertyValueToDomainMappings)
		{
			if (propertyValueToDomainMappings?.TryGetValue(dataPropertyName, out var mapping) ?? false)
			{
				return mapping(value);
			}

			return domainModelPropertyInfo.PropertyType.IsSubclassOf(_typeValueObject)
				? null
				: value;
		}
	}
}