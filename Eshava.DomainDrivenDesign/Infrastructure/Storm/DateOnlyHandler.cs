using System;
using System.Data;
using Eshava.Storm.Handler;
using Eshava.Storm.Interfaces;

namespace Eshava.DomainDrivenDesign.Infrastructure.Storm
{
    public class DateOnlyHandler : TypeHandler<DateOnly>, IBulkInsertTypeHandler
    {
        public Type GetDateType()
        {
            return typeof(DateTime);
        }

        /// <summary>
        /// SQL Server returns a date as DateTime, Npgsql as DateOnly
        /// </summary>
        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                DateOnly dateOnly => dateOnly,
                _ => default
            };
        }

        /// <summary>
        /// DbType.Date is SqlDbType.Date on SqlClient and date on Npgsql, so the handler works with either provider
        /// </summary>
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;

            parameter.Value = value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(0)));
        }
    }
}