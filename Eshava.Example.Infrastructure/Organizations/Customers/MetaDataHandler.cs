using Eshava.Core.Extensions;
using Eshava.Storm.Handler;
using Eshava.Storm.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Text.Json;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class MetaDataHandler : TypeHandler<MetaData>, IBulkInsertTypeHandler
	{
		public Type GetDateType()
		{
			return typeof(MetaData);
		}

		public override MetaData Parse(object value)
		{
			var json = value as string;
			if (json.IsNullOrEmpty())
			{
				return null;
			}

			return JsonSerializer.Deserialize<MetaData>(json);
		}

		public override void SetValue(IDbDataParameter parameter, MetaData value)
		{
			((SqlParameter)parameter).SqlDbType = SqlDbType.NVarChar;

			parameter.Value = value is null
				? null
				: JsonSerializer.Serialize<MetaData>(value);
		}
	}
}