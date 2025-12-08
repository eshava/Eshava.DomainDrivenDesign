using Eshava.Core.Extensions;
using System;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
    public static class TypeExtensions
	{
		private static readonly Type _typeOfString = typeof(string);
		private static readonly Type _typeOfDecimal = typeof(decimal);
		private static readonly Type _typeOfGuid = typeof(Guid);
		private static readonly Type _typeOfDateTime = typeof(DateTime);
		private static readonly Type _typeOfTimeSpan = typeof(TimeSpan);
		private static readonly Type _typeOfByteArray = typeof(byte[]);

		public static bool IsNoClass(this Type type)
		{
			var propertyType = type.GetDataType();

			if (propertyType.IsPrimitive
				|| propertyType.IsEnum
				|| propertyType == _typeOfString
				|| propertyType == _typeOfDecimal
				|| propertyType == _typeOfGuid
				|| propertyType == _typeOfDateTime
				|| propertyType == _typeOfTimeSpan
				|| propertyType == _typeOfByteArray)
			{
				return true;
			}

			return false;
		}

		public static object GetDefault(this Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return null;
		}
	}
}