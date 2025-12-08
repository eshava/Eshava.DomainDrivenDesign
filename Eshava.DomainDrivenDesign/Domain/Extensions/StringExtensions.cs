using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Models;
using System.Collections.Generic;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class StringExtensions
	{
		public static ResponseData<T> ToFaultyResponse<T>(this string message)
		{
			return ResponseData<T>.CreateFaultyResponse(message);
		}

		public static ResponseData<IList<Patch<TDomain>>> ToPatchList<TDomain, TValue>(this string propertyName, TValue propertyValue) where TDomain : class
		{
			var result = propertyName.ToPatch<TDomain, TValue>(propertyValue);
			if (result.IsFaulty)
			{
				return result.ConvertTo<IList<Patch<TDomain>>>();
			}

			var patches = new List<Patch<TDomain>>
			{
				result.Data
			};

			return patches.ToIListResponseData();
		}

		public static ResponseData<Patch<TDomain>> ToPatch<TDomain, TValue>(this string propertyName, TValue propertyValue) where TDomain : class
		{
			var domainType = typeof(TDomain);
			var propertyInfo = domainType.GetProperty(propertyName);
			if (propertyInfo is null)
			{
				return ResponseData<Patch<TDomain>>.CreateInvalidDataResponse()
					.AddValidationError(propertyName, MessageConstants.NOTEXISTING);
			}

			return propertyInfo.ToPatch<TDomain, TValue>(propertyValue).ToResponseData();
		}

		public static string ToPropertyName(this string fieldName)
		{
			if (fieldName.IsNullOrEmpty())
			{
				return fieldName;
			}

			return fieldName.ToUpper()[0] + fieldName.Substring(1);
		}
	}
}