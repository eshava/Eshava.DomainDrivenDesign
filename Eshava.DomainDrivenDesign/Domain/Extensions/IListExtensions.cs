using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class IListExtensions
	{
		public static ResponseData<IList<Patch<TDomain>>> CheckAndConvertValueObjectPatches<TDomain>(this IList<Patch<TDomain>> patches, TDomain domainModel) 
			where TDomain : class
		{
			var validPatchList = new List<Patch<TDomain>>();
			var valueObjectChanges = new Dictionary<string, Dictionary<string, object>>();

			var domainPropertyInfos = domainModel
				.GetType()
				.GetProperties()
				.ToDictionary(p => p.Name, p => p);

			var valueObjectValueCollection = new Dictionary<string, (PropertyInfo Property, Dictionary<string, object> Values)>();

			foreach (var patch in patches)
			{
				var memberExpression = patch.Property.Body.GetMemberExpression();
				if (memberExpression.Expression is ParameterExpression)
				{
					// Domain: p => p.ExamplePropertyName

					validPatchList.Add(patch);
				}
				else if (memberExpression.Expression is MemberExpression parentMemberExpression)
				{
					// The value object must be a direct property of the domain model.
					// Domain:  p => p.ValueObject.ExamplePropertyName

					parentMemberExpression.CheckForValueObject(memberExpression, patch.Value, domainPropertyInfos, valueObjectValueCollection);
				}
			}

			var valueObjectPatches = CreateValueObjectPatches(domainModel, valueObjectValueCollection.Values);
			if (valueObjectPatches is not null)
			{
				validPatchList.AddRange(valueObjectPatches);
			}

			return validPatchList
				.ToIListResponseData();
		}

		/// <summary>
		/// Creates instances of the value objects from the individual property values of the value objects and uses them to create a patch for the domain model.
		/// </summary>
		/// <typeparam name="TDomain"></typeparam>
		/// <param name="valueObjectValueCollection"></param>
		/// <returns></returns>
		private static IEnumerable<Patch<TDomain>> CreateValueObjectPatches<TDomain>(TDomain domainModel, IEnumerable<(PropertyInfo Property, Dictionary<string, object> Values)> valueObjectValueCollection) 
			where TDomain : class
		{
			if (!valueObjectValueCollection.Any())
			{
				return null;
			}

			var patches = new List<Patch<TDomain>>();

			foreach (var valueObjectParts in valueObjectValueCollection)
			{
				var valueObject = valueObjectParts.Property.GetValue(domainModel);
				var valueObjectProperties = valueObjectParts.Property
					.PropertyType
					.GetProperties()
					.ToDictionary(p => p.Name, p => p);

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
						if (valueObject is null)
						{
							parameterValue = Expression.Default(parameter.ParameterType);
						}
						else
						{
							parameterValue = valueObjectProperties[propertyName].GetValue(valueObject);
						}
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