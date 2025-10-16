using Eshava.Storm.Handler;
using Eshava.Storm.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Eshava.DomainDrivenDesign.Infrastructure.Storm
{
    public class TimeOnlyHandler : TypeHandler<TimeOnly>, IBulkInsertTypeHandler
    {
        public Type GetDateType()
        {
            return typeof(TimeSpan);
        }

        public override TimeOnly Parse(object value)
        {
            if (value is TimeSpan timeSpan)
            {
                return TimeOnly.FromTimeSpan(timeSpan);
            }

            return default;
        }

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            ((SqlParameter)parameter).SqlDbType = SqlDbType.Time;

            parameter.Value = value.ToTimeSpan();
        }
    }
}