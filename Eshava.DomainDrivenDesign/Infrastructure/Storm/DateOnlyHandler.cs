using System;
using System.Data;
using Eshava.Storm.Handler;
using Eshava.Storm.Interfaces;
using Microsoft.Data.SqlClient;

namespace Eshava.DomainDrivenDesign.Infrastructure.Storm
{
    public class DateOnlyHandler : TypeHandler<DateOnly>, IBulkInsertTypeHandler
    {
        public Type GetDateType()
        {
            return typeof(DateTime);
        }

        public override DateOnly Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }

            return default;
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            ((SqlParameter)parameter).SqlDbType = SqlDbType.Date;

            parameter.Value = value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(0)));
        }
    }
}