using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Settings;
using Eshava.Storm;
using Eshava.Storm.Interfaces;
using Eshava.Storm.Linq.Extensions;
using Eshava.Storm.Linq.Models;
using Microsoft.Extensions.Logging;

namespace Eshava.DomainDrivenDesign.Infrastructure.Repositories
{
	public abstract class AbstractQueryRepository<TIdentifier> : AbstractRepository
		where TIdentifier : struct
	{
		public AbstractQueryRepository(
			IDatabaseSettings databaseSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger logger
		) : base(transformQueryEngine)
		{
			DatabaseSettings = databaseSettings;
			Logger = logger;
		}

		protected IDatabaseSettings DatabaseSettings { get; }
		protected ILogger Logger { get; }
		protected Dictionary<Type, string> PropertyTypeMappings { get; set; }

		protected Task<IEnumerable<TData>> FilterAsync<TData>(QueryWrapper sqlQuery, FilterRequestDto<TData> filterRequest, WhereQuerySettings settings = null)
		   where TData : class
		{
			return FilterAsync<TData, TData>(sqlQuery, filterRequest, settings, null);
		}

		protected async Task<IEnumerable<TReturn>> FilterAsync<TData, TReturn>(QueryWrapper sqlQuery, FilterRequestDto<TData> filterRequest, WhereQuerySettings settings = null, Func<IObjectMapper, TReturn> queryMapper = null)
			where TData : class
			where TReturn : class
		{
			using (var connection = DatabaseSettings.GetConnection())
			{
				if (settings == null && PropertyTypeMappings != null)
				{
					settings = new WhereQuerySettings
					{
						PropertyTypeMappings = PropertyTypeMappings
					};
				}

				var queryBuilderResult = filterRequest.Where.AddWhereConditionsToQuery(sqlQuery.Query, settings);
				sqlQuery.Query = queryBuilderResult.Sql;

				var sharedSettings = settings as SharedWhereQuerySettings;
				if (!(sharedSettings?.QueryPartBetweenWhereAndOrderBy.IsNullOrEmpty() ?? true))
				{
					sqlQuery.Query += Environment.NewLine + sharedSettings.QueryPartBetweenWhereAndOrderBy;
				}

				sqlQuery.Query = filterRequest.Sort.AddSortConditionsToQuery(sqlQuery.Query, settings);
				sqlQuery.Query = sqlQuery.Query.AppendSkipAndTake(filterRequest.Skip, filterRequest.Take);

				return queryMapper == null
					? await connection.QueryAsync<TReturn>(sqlQuery.Query, queryBuilderResult.QueryParameter)
					: await connection.QueryAsync(sqlQuery.Query, queryMapper, queryBuilderResult.QueryParameter);
			}
		}

		protected async Task<int> FilterCountAsync<TData>(QueryWrapper sqlQuery, FilterRequestDto<TData> filterRequest, WhereQuerySettings settings = null) where TData : class
		{
			using (var connection = DatabaseSettings.GetConnection())
			{
				if (settings == null && PropertyTypeMappings != null)
				{
					settings = new WhereQuerySettings
					{
						PropertyTypeMappings = PropertyTypeMappings
					};
				}

				var queryBuilderResult = filterRequest.Where.AddWhereConditionsToQuery(sqlQuery.Query, settings);
				sqlQuery.Query = queryBuilderResult.Sql;

				var sharedSettings = settings as SharedWhereQuerySettings;
				if (!(sharedSettings?.QueryPartBetweenWhereAndOrderBy.IsNullOrEmpty() ?? true))
				{
					sqlQuery.Query += Environment.NewLine + sharedSettings.QueryPartBetweenWhereAndOrderBy;
				}

				return await connection.ExecuteScalarAsync<int>(sqlQuery.Query, queryBuilderResult.QueryParameter);
			}
		}
	}
}