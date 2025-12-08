using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class ExpressionExtensions
	{
		private static readonly Type _typeValueObject = typeof(AbstractValueObject);

		public static Expression<Func<T, TargetPropertyType>> ConvertMemberExpressionFunction<T, SourcePropertyType, TargetPropertyType>(this Expression<Func<T, SourcePropertyType>> expression) where T : class
		{
			if (typeof(TargetPropertyType) == typeof(SourcePropertyType))
			{
				return Expression.Lambda<Func<T, TargetPropertyType>>(expression.Body, expression.Parameters.First());
			}

			return expression.Body
				.GetMemberExpression()
				.ConvertToMemberExpressionFunction<T, TargetPropertyType>(expression.Parameters.First());
		}

		public static Expression<Func<T, TargetPropertyType>> ConvertToMemberExpressionFunction<T, TargetPropertyType>(this MemberExpression memberExpression, ParameterExpression parameter) where T : class
		{
			var expression = Expression.Convert(memberExpression, typeof(TargetPropertyType));

			return Expression.Lambda<Func<T, TargetPropertyType>>(expression, parameter);
		}

		public static string GetMemberExpressionString<T>(this Expression<Func<T, object>> expression) where T : class
		{
			var memberExpression = GetMemberExpression(expression.Body);

			return GetMemberExpressionString(memberExpression);
		}

		public static string GetMemberExpressionString(this MemberExpression memberExpression, bool lastMemberExpressionPart = false)
		{
			if (memberExpression == null)
			{
				return null;
			}

			var memberExpressionString = memberExpression.ToString();

			return lastMemberExpressionPart
				? memberExpressionString.Substring(memberExpressionString.LastIndexOf(".") + 1)
				: memberExpressionString.Substring(memberExpressionString.IndexOf(".") + 1)
				;
		}

		public static MemberExpression GetMemberExpression(this Expression expression)
		{
			MemberExpression memberExpression;

			do
			{
				memberExpression = expression as MemberExpression;
				if (memberExpression == null)
				{
					expression = (expression as UnaryExpression)?.Operand;
				}

			} while (memberExpression == null && expression != null);

			return memberExpression;
		}

		public static ResponseData<bool> SetPropertyValue<T>(this Expression<Func<T, object>> sourceExpression, object root, object value) where T : class
		{
			try
			{
				var sourceMemberExpression = GetMemberExpression(sourceExpression.Body);

				var parent = sourceMemberExpression.Expression.GetObjectInstance(root, true);
				var propertyInfo = parent.GetType().GetProperty(sourceMemberExpression.Member.Name);

				var currentValue = propertyInfo.GetValue(parent, null);
				if (Equals(currentValue, value))
				{
					return false.ToResponseData();
				}

				propertyInfo.SetValue(parent, value ?? GetDefault(propertyInfo.PropertyType));

				return true.ToResponseData();
			}
			catch (Exception ex)
			{
				return ResponseData<bool>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex);
			}
		}

		public static object GetObjectInstance(this Expression expression, object root, bool setInstanceIfNull)
		{
			if (expression is ParameterExpression)
			{
				return root;
			}

			if (expression is MemberExpression)
			{
				var m = expression as MemberExpression;
				var parent = GetObjectInstance(m.Expression, root, setInstanceIfNull);
				if (parent == null)
				{
					return null;
				}

				var propertyInfo = parent.GetType().GetProperty(m.Member.Name);
				var instance = propertyInfo.GetValue(parent);

				if (instance == default && setInstanceIfNull && !propertyInfo.PropertyType.IsNoClass())
				{
					instance = propertyInfo.PropertyType.CreateInstance();
					propertyInfo.SetValue(parent, instance);
				}

				return instance;
			}

			return null;
		}

		/// <summary>
		/// Collects all dto property values belonging to a value object
		/// </summary>
		/// <param name="parentMemberExpression">p.ValueObject</param>
		/// <param name="memberExpression">p.ValueObject.ExamplePropertyName</param>
		/// <param name="patchValue"></param>
		/// <param name="domainPropertyInfos"></param>
		/// <param name="valueObjectValueCollection"></param>
		public static void CheckForValueObject(
			this MemberExpression parentMemberExpression,
			MemberExpression memberExpression,
			object patchValue,
			Dictionary<string, PropertyInfo> domainPropertyInfos,
			Dictionary<string, (PropertyInfo Property, Dictionary<string, object> Values)> valueObjectValueCollection
		)
		{
			var propertyName = memberExpression.GetMemberExpressionString(true);
			var objectPropertyName = parentMemberExpression.GetMemberExpressionString();
			if (domainPropertyInfos.TryGetValue(objectPropertyName, out var domainProperty) && domainProperty.PropertyType.IsSubclassOf(_typeValueObject))
			{
				if (!valueObjectValueCollection.TryGetValue(domainProperty.Name, out var valueObjectContainer))
				{
					valueObjectContainer = (domainProperty, new Dictionary<string, object>());
					valueObjectValueCollection.Add(domainProperty.Name, valueObjectContainer);
				}

				if (!valueObjectContainer.Values.TryAdd(propertyName, patchValue))
				{
					valueObjectContainer.Values[propertyName] = patchValue;
				}
			}
		}

		private static object GetDefault(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return null;
		}
	}
}