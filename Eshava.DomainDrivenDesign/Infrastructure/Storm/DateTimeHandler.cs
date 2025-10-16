using Eshava.Storm.Handler;
using System;
using System.Data;

namespace Eshava.DomainDrivenDesign.Infrastructure.Storm
{
    public class DateTimeHandler : TypeHandler<DateTime>
	{
		public override void SetValue(IDbDataParameter parameter, DateTime value)
		{
			parameter.Value = value.ToUniversalTime();
		}

		public override DateTime Parse(object value)
		{
			return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
		}
	}
}