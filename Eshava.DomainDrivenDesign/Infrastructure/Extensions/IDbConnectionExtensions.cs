using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eshava.Storm;
using Eshava.Storm.Linq.Extensions;
using Eshava.Storm.MetaData;


namespace Eshava.DomainDrivenDesign.Infrastructure.Extensions
{
   public static class IDbConnectionExtensions
	{
		public static Task<T> QueryEntityAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> queryCondition, IDbTransaction transaction = null, int? commandTimeout = null, CancellationToken cancellationToken = default) where T : class
		{
			var baseQuery = GetBaseQuery<T>();
			var queryConditions = new[] { queryCondition };
			var queryResult = queryConditions.AddWhereConditionsToQuery(baseQuery);

			return connection.QueryFirstOrDefaultAsync<T>(queryResult.Sql, queryResult.QueryParameter, transaction, commandTimeout, CommandType.Text, cancellationToken);
		}

		public static Task<IEnumerable<T>> QueryEntitiesAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> queryCondition, IDbTransaction transaction = null, int? commandTimeout = null, CancellationToken cancellationToken = default) where T : class
		{
			var baseQuery = GetBaseQuery<T>();
			var queryConditions = new[] { queryCondition };
			var queryResult = queryConditions.AddWhereConditionsToQuery(baseQuery);

			return connection.QueryAsync<T>(queryResult.Sql, queryResult.QueryParameter, transaction, commandTimeout, CommandType.Text, cancellationToken);
		}

		private static string GetBaseQuery<T>() where T : class
		{
			return $"SELECT * FROM {TypeAnalyzer.GetTableName<T>()}";
		}
	}
}