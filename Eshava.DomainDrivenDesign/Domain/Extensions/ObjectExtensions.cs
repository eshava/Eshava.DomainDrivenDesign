using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class ObjectExtensions
	{
		private static readonly Type _typeValueObject = typeof(AbstractValueObject);

		public static IList<Patch<TDomain>> ToPatches<TDto, TDomain>(this TDto dto, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<TDomain, object>> Domain)> mappings = null)
			where TDto : class
			where TDomain : class
		{
			var patches = new List<Patch<TDomain>>();
			var dtoType = typeof(TDto);
			var domainType = typeof(TDomain);

			var dtoPropertyInfos = dtoType.GetProperties();
			var domainPropertyInfos = domainType
				.GetProperties()
				.ToDictionary(p => p.Name, p => p);

			var valueObjectValueCollection = new Dictionary<string, (PropertyInfo Property, Dictionary<string, object> Values)>();

			foreach (var dtoPropertyInfo in dtoPropertyInfos)
			{
				var dtoValue = dtoPropertyInfo.GetValue(dto, null);

				if (mappings is not null && dtoValue is not null)
				{
					var mapping = mappings.FirstOrDefault(m => m.Dto.GetMemberExpressionString() == dtoPropertyInfo.Name);
					if (mapping.Dto is not null)
					{
						var memberExpression = mapping.Domain.Body as MemberExpression;
						if (memberExpression.Expression is ParameterExpression)
						{
							// Domain: p => p.ExamplePropertyName
							if (mapping.Domain.GetMemberExpressionString() == "Id" && Equals(dtoValue, dtoPropertyInfo.PropertyType.GetDefault()))
							{
								// Skip default value if it's the identifier property

								continue;
							}

							patches.Add(Patch<TDomain>.Create(mapping.Domain, dtoValue));
						}
						else if (memberExpression.Expression is MemberExpression parentMemberExpression)
						{
							// The value object must be a direct property of the domain model.
							// Domain:  p => p.ValueObject.ExamplePropertyName

							parentMemberExpression.CheckForValueObject(memberExpression, dtoValue, domainPropertyInfos, valueObjectValueCollection);
						}

						continue;
					}

					if (!dtoPropertyInfo.PropertyType.IsNoClass())
					{
						// The value object must be a direct property of the domain model.
						// Dto: p => p.ValueObjectDto.ExamplePropertyName
						// Domain: p => p.ValueObject.ExamplePropertyName

						var valueObjectResult = CheckForValueObject(dtoPropertyInfo.Name, dtoValue, domainPropertyInfos, mappings);
						if (valueObjectResult.Property is not null)
						{
							valueObjectValueCollection.Add(valueObjectResult.Property.Name, valueObjectResult);

							continue;
						}
					}
				}

				if (dtoPropertyInfo.Name == "Id" && (dtoValue is null || Equals(dtoValue, dtoPropertyInfo.PropertyType.GetDefault())))
				{
					// Skip default value if it's the identifier property

					continue;
				}

				if (!domainPropertyInfos.TryGetValue(dtoPropertyInfo.Name, out var domainPropertyInfo) || !domainPropertyInfo.CanWrite)
				{
					continue;
				}

				patches.Add(domainPropertyInfo.ToPatch<TDomain, object>(dtoValue));
			}

			var valueObjectPatches = CreateValueObjectPatches<TDomain>(valueObjectValueCollection.Values);
			if (valueObjectPatches is not null)
			{
				patches.AddRange(valueObjectPatches);
			}

			return patches;
		}

		/// <summary>
		/// Checks if there are property mappings for the dto property value (class instance)
		/// </summary>
		/// <typeparam name="TDto"></typeparam>
		/// <typeparam name="TDomain"></typeparam>
		/// <param name="dtoPropertyName"></param>
		/// <param name="dtoValue"></param>
		/// <param name="domainPropertyInfos"></param>
		/// <param name="mappings"></param>
		/// <returns></returns>
		private static (PropertyInfo Property, Dictionary<string, object> Values) CheckForValueObject<TDto, TDomain>(
			string dtoPropertyName,
			object dtoValue,
			Dictionary<string, PropertyInfo> domainPropertyInfos,
			IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<TDomain, object>> Domain)> mappings
		)
			where TDto : class
			where TDomain : class
		{
			var possibleMappings = mappings.Where(m => m.Dto.GetMemberExpressionString().Contains(dtoPropertyName)).ToList();
			if (possibleMappings.Count > 0)
			{
				var memberExpression = (possibleMappings[0].Domain.Body as MemberExpression)?.Expression as MemberExpression;
				if (memberExpression is not null)
				{
					var objectPropertyName = memberExpression.GetMemberExpressionString();
					if (domainPropertyInfos.TryGetValue(objectPropertyName, out var domainProperty) && domainProperty.PropertyType.IsSubclassOf(_typeValueObject))
					{
						var valueObjectValues = ToValueObjectValues(dtoValue, domainProperty, possibleMappings);

						return (domainProperty, valueObjectValues);
					}
				}
			}

			return (null, null);
		}

		/// <summary>
		/// Convert the dto value (class instance) in property values of the value object 
		/// </summary>
		/// <typeparam name="TDto"></typeparam>
		/// <typeparam name="TDomain"></typeparam>
		/// <param name="valueObjectDto"></param>
		/// <param name="domainModelProperty"></param>
		/// <param name="mappings"></param>
		/// <returns></returns>
		private static Dictionary<string, object> ToValueObjectValues<TDto, TDomain>(object valueObjectDto, PropertyInfo domainModelProperty, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<TDomain, object>> Domain)> mappings)
			where TDto : class
			where TDomain : class
		{
			var valueObjectValues = new Dictionary<string, object>();
			var dtoType = valueObjectDto.GetType();
			var dtoPropertyInfos = dtoType.GetProperties();

			foreach (var dtoPropertyInfo in dtoPropertyInfos)
			{
				var dtoValue = dtoPropertyInfo.GetValue(valueObjectDto, null);

				var mapping = mappings.FirstOrDefault(m => m.Dto.GetMemberExpressionString().EndsWith("." + dtoPropertyInfo.Name));
				if (mapping.Dto is not null)
				{
					var memberExpression = mapping.Domain.Body as MemberExpression;
					var propertyName = memberExpression.GetMemberExpressionString(true);

					valueObjectValues.Add(propertyName, dtoValue);
				}
			}

			return valueObjectValues;
		}

		/// <summary>
		/// Creates instances of the value objects from the individual property values of the value objects and uses them to create a patch for the domain model.
		/// </summary>
		/// <typeparam name="TDomain"></typeparam>
		/// <param name="valueObjectValueCollection"></param>
		/// <returns></returns>
		private static IEnumerable<Patch<TDomain>> CreateValueObjectPatches<TDomain>(IEnumerable<(PropertyInfo Property, Dictionary<string, object> Values)> valueObjectValueCollection) where TDomain : class
		{
			if (!valueObjectValueCollection.Any())
			{
				return null;
			}

			var patches = new List<Patch<TDomain>>();

			foreach (var valueObjectParts in valueObjectValueCollection)
			{
				var constructorParameterValues = new List<object>();
				var constructorParameter = valueObjectParts
					.Property
					.PropertyType
					.GetConstructors()[0]
					.GetParameters();

				foreach (var parameter in constructorParameter)
				{
					// Assuming naming convention
					// Except for the first letter, the property name and the parameter name must be identical (case sensitive).
					// The first letter may only differ in upper and lower case.
					var propertyName = parameter.Name.ToPropertyName();
					if (!valueObjectParts.Values.TryGetValue(propertyName, out var parameterValue))
					{
						parameterValue = Expression.Default(parameter.ParameterType);
					}

					constructorParameterValues.Add(parameterValue);
				}

				var valueObjectInstance = valueObjectParts.Property.PropertyType.CreateInstance(constructorParameterValues.ToArray());

				patches.Add(valueObjectParts.Property.ToPatch<TDomain, object>(valueObjectInstance));
			}

			return patches;
		}
	}
}