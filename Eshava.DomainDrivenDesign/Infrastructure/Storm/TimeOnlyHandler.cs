using Eshava.Storm.Handler;
using Eshava.Storm.Interfaces;
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

        /// <summary>
        /// SQL Server returns a time as TimeSpan, Npgsql as TimeOnly
        /// </summary>
        public override TimeOnly Parse(object value)
        {
            return value switch
            {
                TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
                TimeOnly timeOnly => timeOnly,
                _ => default
            };
        }

        /// <summary>
        /// DbType.Time is SqlDbType.Time on SqlClient and time on Npgsql, so the handler works with either provider
        /// </summary>
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;

            parameter.Value = value.ToTimeSpan();
        }
    }
}