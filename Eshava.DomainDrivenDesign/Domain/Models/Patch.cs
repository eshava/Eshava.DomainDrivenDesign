using System;
using System.Linq;
using System.Linq.Expressions;
using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Domain.Extensions;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public class Patch<TDomain> where TDomain : class
	{
		private static Type _typeOfString = typeof(string);
		private static Type _typeOfInt = typeof(int);

		private Patch(Expression<Func<TDomain, object>> property, object value)
		{
			Property = property;
			Value = value;
			PropertyName = property.GetMemberExpressionString();
		}

		public string PropertyName { get; }
		public Expression<Func<TDomain, object>> Property { get; }
		public object Value { get; }

		/// <summary>
		/// Creates a typed patch property instance
		/// </summary>
		/// <typeparam name="TPropertyType"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Patch<TDomain> Create<TPropertyType>(Expression<Func<TDomain, TPropertyType>> property, TPropertyType value)
		{
			if (property.Body is MemberExpression)
			{
				var expression = property.ConvertMemberExpressionFunction<TDomain, TPropertyType, object>();
				if (typeof(TPropertyType) == _typeOfString && value is not null)
				{
					return new Patch<TDomain>(expression, (value as string).Trim());
				}

				return new Patch<TDomain>(expression, value);
			}

			var valueObject = (object)value;
			var memberExpression = CheckDataType(property.Body, ref valueObject);
			var propertyExpression = memberExpression.ConvertToMemberExpressionFunction<TDomain, object>(property.Parameters.First());

			return new Patch<TDomain>(propertyExpression, (TPropertyType)valueObject);
		}

		public static Patch<TDomain> Create(Expression<Func<TDomain, object>> property, object value)
		{
			CheckDataType(property.Body, ref value);

			return new Patch<TDomain>(property, value);
		}

		private static MemberExpression CheckDataType(Expression expression, ref object value)
		{
			var memberExpression = expression.GetMemberExpression();
			if (memberExpression == null)
			{
				return null;
			}

			if (value == null)
			{
				if (memberExpression.Type.IsNoClass() && !memberExpression.Type.IsDataTypeNullable() && memberExpression.Type != _typeOfString)
				{
					throw new ArgumentNullException(FormatExceptionMessage(memberExpression, null));
				}
			}
			else
			{
				var propertyType = memberExpression.Type.GetDataType();
				var valueType = value.GetType();

				if (propertyType != valueType
					&& !(propertyType.IsEnum && valueType == _typeOfInt)
					&& !(propertyType == _typeOfInt && valueType.IsEnum)
					&& !(propertyType.ImplementsIEnumerable() && valueType.ImplementsIEnumerable()))
				{

					throw new ArgumentException(FormatExceptionMessage(memberExpression, value.GetType()));
				}

				if (propertyType == _typeOfString)
				{
					value = value.ToString().Trim();
				}
			}

			return memberExpression;
		}

		private static string FormatExceptionMessage(MemberExpression memberExpression, Type valueType)
		{
			return valueType == null
				? $"{typeof(TDomain).Name}.{memberExpression.GetMemberExpressionString()}"
				: $"{typeof(TDomain).Name}.{memberExpression.GetMemberExpressionString()}: {valueType.Name}"
				;
		}
	}
}