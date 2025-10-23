using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Enums;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Models;
using Eshava.Example.Application.Settings;
using Eshava.Storm;
using Eshava.Storm.Linq.Models;
using Eshava.Storm.MetaData;
using Microsoft.Extensions.Logging;
using DTOS_READ = Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read;
using DTOS_SEARCH = Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class OfficeQueryRepository : AbstractExampleQueryRepository<Office, int>, IOfficeQueryRepository
	{
		private Dictionary<Type, string> _propertyTypeMappings = new Dictionary<Type, string>
		{
			{
				typeof(Office),
				OFFICE
			},
			{
				typeof(Customers.Customer),
				CUSTOMER
			}
		};
		private readonly ExampleScopedSettings _scopedSettings;
		private const string CUSTOMER = "customer";
		private const string OFFICE = "office";

		public OfficeQueryRepository(
			ExampleScopedSettings scopedSettings,
			IDatabaseSettings databaseSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger<OfficeQueryRepository> logger
		) : base(databaseSettings, transformQueryEngine, logger)
		{
			_scopedSettings = scopedSettings;
			PropertyTypeMappings = _propertyTypeMappings;
		}

		public async Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, int? officeId, string name)
		{
			try
			{
				var query = $"""

					SELECT
						 COUNT({OFFICE}.{nameof(Office.Id)})
					FROM
						{TypeAnalyzer.GetTableName<Office>()} {OFFICE}
					WHERE
						{OFFICE}.{nameof(Office.Name)} = @Name
					AND
						{OFFICE}.{nameof(Office.Status)} = @Status
						
					""";

				if (customerId.HasValue)
				{
					query += $"""

					AND
						{OFFICE}.{nameof(Office.CustomerId)} != @CustomerId
					""";
				}

				if (officeId.HasValue)
				{
					query += $"""

					AND
						{OFFICE}.{nameof(Office.Id)} != @OfficeId
					""";
				}

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.ExecuteScalarAsync<int>(query, new
					{
						CustomerId = customerId,
						OfficeId = officeId,
						Name = name,
						Status = Status.Active
					});

					return (result == 0).ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, "Entity Office could not be checked", ex, additional: new
				{
					OfficeId = officeId,
					Name = name
				});

				return ResponseData<bool>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<bool>> ExistsAsync(int officeId)
		{
			try
			{
				var query = $"""

					SELECT
						 COUNT({OFFICE}.{nameof(Office.Id)})
					FROM
						{TypeAnalyzer.GetTableName<Office>()} {OFFICE}
					WHERE
						{OFFICE}.{nameof(Office.Id)} = @OfficeId
					AND
						{OFFICE}.{nameof(Office.Status)} = @Status
						
				""";

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.ExecuteScalarAsync<int>(query, new { OfficeId = officeId, Status = Status.Active });

					return (result > 0).ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, "Entity Office could not be checked", ex, additional: new
				{
					OfficeId = officeId
				});

				return ResponseData<bool>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<DTOS_READ.OfficeReadDto>> ReadAsync(int office)
		{
			try
			{
				var query = $"""

				SELECT
					 {OFFICE}.{nameof(Office.Id)}
					,{OFFICE}.{nameof(Office.CustomerId)}
					,{OFFICE}.{nameof(Office.Name)}
				FROM
					 {TypeAnalyzer.GetTableName<Office>()} {OFFICE}
				WHERE
					{OFFICE}.{nameof(Office.Id)} = @OfficeId
				AND
					{OFFICE}.{nameof(Office.Status)} = @Status
					
				""";

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.QueryFirstOrDefaultAsync(query, mapper =>
					{
						var dto = new DTOS_READ.OfficeReadDto
						{
							Id = mapper.GetValue<int>(nameof(Office.Id), OFFICE),
							CustomerId = mapper.GetValue<int>(nameof(Office.CustomerId), OFFICE),
							Name = mapper.GetValue<string>(nameof(Office.Name), OFFICE)
						};

						return dto;
					},
					new { OfficeId = office, Status = Status.Active });

					if (result is null)
					{
						return new ResponseData<DTOS_READ.OfficeReadDto>();
					}

					return result.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, "Entity Office could not be read", ex, additional: new
				{
					OfficeId = office
				});

				return ResponseData<DTOS_READ.OfficeReadDto>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<IEnumerable<DTOS_SEARCH.OfficeSearchDto>>> SearchAsync(FilterRequestDto<DTOS_SEARCH.OfficeSearchDto> searchRequest)
		{
			var query = new QueryWrapper();

			try
			{
				var dbFilterRequest = TransformFilterRequest<DTOS_SEARCH.OfficeSearchDto, Office>(searchRequest);
				dbFilterRequest = AddStatusQueryConditions(dbFilterRequest);

				query.Query = $"""

					SELECT
						  {CUSTOMER}.*
						 ,{OFFICE}.*
					FROM
						{TypeAnalyzer.GetTableName<Office>()} {OFFICE}
					JOIN 
						{TypeAnalyzer.GetTableName<Customers.Customer>()} {CUSTOMER} ON {CUSTOMER}.{nameof(Customers.Customer.Id)} = {OFFICE}.{nameof(Office.CustomerId)}
																				   AND {CUSTOMER}.{nameof(Customers.Customer.Status)} = @Status
					
					""";

				var settings = new WhereQuerySettings
				{
					PropertyTypeMappings = PropertyTypeMappings,
					QueryParameter = new Dictionary<string, object>
					{
						{ "Status", Status.Active }
					}
				};

				var result = await FilterAsync(query, dbFilterRequest, settings, mapper =>
				{
					var office = mapper.Map<Office>(OFFICE);
					office.Customer = mapper.Map<Customers.Customer>(CUSTOMER);

					return office;
				});

				var dtos = result
					.Select(entity => new DTOS_SEARCH.OfficeSearchDto
					{
						Id = entity.Id,
						CustomerId = entity.CustomerId,
						CustomerName = entity.Customer.CompanyName,
						CustomerClassification = entity.Customer.Classification,
						Name = entity.Name
					})
					.ToList();

				return dtos.ToIEnumerableResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, $"Entity {typeof(Office).Name} could not be read.", ex, additional: new
				{
					SqlQuery = query.Query
				});

				return ResponseData<IEnumerable<DTOS_SEARCH.OfficeSearchDto>>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<DTOS_SEARCH.OfficeSearchDto> searchRequest)
		{
			var query = new QueryWrapper();
			try
			{
				var dbFilterRequest = TransformFilterRequest<DTOS_SEARCH.OfficeSearchDto, Office>(searchRequest);
				dbFilterRequest = AddStatusQueryConditions(dbFilterRequest);
				query.Query = $"""

				SELECT
					 COUNT({OFFICE}.{nameof(Office.Id)})
				FROM
						{TypeAnalyzer.GetTableName<Office>()} {OFFICE}
					JOIN 
						{TypeAnalyzer.GetTableName<Customers.Customer>()} {CUSTOMER} ON {CUSTOMER}.{nameof(Customers.Customer.Id)} = {OFFICE}.{nameof(Office.CustomerId)}
																				   AND {CUSTOMER}.{nameof(Customers.Customer.Status)} = @Status
				""";

				var settings = new WhereQuerySettings
				{
					PropertyTypeMappings = PropertyTypeMappings,
					QueryParameter = new Dictionary<string, object>()
				};

				settings.QueryParameter.Add("Status", Status.Active);
				var result = await FilterCountAsync(query, dbFilterRequest, settings);

				return result.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, $"Entity {typeof(Office).Name}  could not be read", ex, additional: new
				{
					SqlQuery = query.Query
				});

				return ResponseData<int>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}
	}
}