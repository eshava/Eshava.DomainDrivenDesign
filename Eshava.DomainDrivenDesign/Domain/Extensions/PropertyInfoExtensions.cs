using System.Linq.Expressions;
using System.Reflection;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class PropertyInfoExtensions
	{
		public static Patch<TDomain> ToPatch<TDomain, TValue>(this PropertyInfo propertyInfo, TValue propertyValue) where TDomain : class
		{
			var domainType = typeof(TDomain);
			var parameter = Expression.Parameter(domainType, "p");
			var member = Expression.MakeMemberAccess(parameter, propertyInfo);
			var expressionFunction = member.ConvertToMemberExpressionFunction<TDomain, TValue>(parameter);

			return Patch<TDomain>.Create(expressionFunction, propertyValue);
		}
	}
}