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
using DTOS_READ = Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read;
using DTOS_SEARCH = Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;


namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerQueryRepository : AbstractExampleQueryRepository<Customer, int>, ICustomerQueryRepository
	{
		private Dictionary<Type, string> _propertyTypeMappings = new Dictionary<Type, string>
		{
			{
				typeof(Customer),
				CUSTOMER
			},
			{
				typeof(Offices.Office),
				OFFICE
			}
		};

		private readonly ExampleScopedSettings _scopedSettings;
		private const string CUSTOMER = "customer";
		private const string OFFICE = "office";

		public CustomerQueryRepository(
			ExampleScopedSettings scopedSettings,
			IDatabaseSettings databaseSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger<CustomerQueryRepository> logger
		) : base(databaseSettings, transformQueryEngine, logger)
		{
			_scopedSettings = scopedSettings;
			PropertyTypeMappings = _propertyTypeMappings;
		}

		public async Task<ResponseData<bool>> IsUniqueNameAsync(int? customerId, string name)
		{
			try
			{
				var query = $"""

					SELECT
						 COUNT({CUSTOMER}.{nameof(Customer.Id)})
					FROM
						{TypeAnalyzer.GetTableName<Customer>()} {CUSTOMER}
					WHERE
						{CUSTOMER}.{nameof(Customer.CompanyName)} = @CompanyName
					AND
						{CUSTOMER}.{nameof(Customer.Status)} = @Status
						
					""";

				if (customerId.HasValue)
				{
					query += $"""

					AND
						{CUSTOMER}.{nameof(Customer.Id)} != @CustomerId
					""";
				}

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.ExecuteScalarAsync<int>(query, new
					{
						CustomerId = customerId,
						CompanyName = name,
						Status = Status.Active
					});

					return (result == 0).ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, "Entity Customer could not be checked", ex, additional: new
				{
					CustomerId = customerId,
					CompanyName = name
				});

				return ResponseData<bool>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<DTOS_READ.CustomerReadDto>> ReadAsync(int customerId)
		{
			try
			{
				var query = $"""

				SELECT
					 {CUSTOMER}.{nameof(Customer.Id)}
					,{CUSTOMER}.{nameof(Customer.CompanyName)}
					,{CUSTOMER}.{nameof(Customer.Classification)}
					,{OFFICE}.{nameof(Offices.Office.Id)}
					,{OFFICE}.{nameof(Offices.Office.Name)}
				FROM
					 {TypeAnalyzer.GetTableName<Customer>()} {CUSTOMER}
				LEFT JOIN
					 {TypeAnalyzer.GetTableName<Offices.Office>()} {OFFICE}
						ON {OFFICE}.{nameof(Offices.Office.CustomerId)} = {CUSTOMER}.{nameof(Customer.Id)}
						AND {OFFICE}.{nameof(Offices.Office.Status)} = @Status
				WHERE
					{CUSTOMER}.{nameof(Customer.Id)} = @CustomerId
				AND
					{CUSTOMER}.{nameof(Customer.Status)} = @Status
					
				""";

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.QueryAsync(query, mapper =>
					{
						var dto = new DTOS_READ.CustomerReadDto
						{
							Id = mapper.GetValue<int>(nameof(Customer.Id), CUSTOMER),
							Name = mapper.GetValue<string>(nameof(Customer.CompanyName), CUSTOMER),
							Classification = mapper.GetValue<Domain.Organizations.SomeFeature.Classification>(nameof(Customer.Classification), CUSTOMER),
							Offices = mapper.GetValue<int>(nameof(Offices.Office.Id), OFFICE) == default
								? new List<DTOS_READ.CustomerReadOfficeDto>()
								: new List<DTOS_READ.CustomerReadOfficeDto>
								{
									new DTOS_READ.CustomerReadOfficeDto
									{
										Id = mapper.GetValue<int>(nameof(Offices.Office.Id), OFFICE),
										Name = mapper.GetValue<string>(nameof(Offices.Office.Name), OFFICE),
									}
								}
						};

						return dto;
					},
					new { CustomerId = customerId, Status = Status.Active });

					if (!result.Any())
					{
						return new ResponseData<DTOS_READ.CustomerReadDto>();
					}

					var customerDto = result.First();
					customerDto.Offices = result
						.Where(c => c.Offices is not null)
						.SelectMany(c => c.Offices)
						.ToList();

					return customerDto.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, "Entity Customer could not be read", ex, additional: new
				{
					CustomerId = customerId
				});

				return ResponseData<DTOS_READ.CustomerReadDto>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<IEnumerable<DTOS_SEARCH.CustomerSearchDto>>> SearchAsync(FilterRequestDto<DTOS_SEARCH.CustomerSearchDto> searchRequest)
		{
			var query = new QueryWrapper();

			try
			{
				var dbFilterRequest = TransformFilterRequest<DTOS_SEARCH.CustomerSearchDto, Customer>(searchRequest);
				dbFilterRequest = AddStatusQueryConditions(dbFilterRequest);

				query.Query = $"""

					SELECT
						  {CUSTOMER}.*
					FROM
						{TypeAnalyzer.GetTableName<Customer>()} {CUSTOMER}
					
					""";

				var settings = new WhereQuerySettings
				{
					PropertyTypeMappings = PropertyTypeMappings
				};

				var result = await FilterAsync(query, dbFilterRequest, settings, mapper =>
				{
					var customer = mapper.Map<Customer>(CUSTOMER);

					return customer;
				});

				var dtos = result
					.Select(entity => new DTOS_SEARCH.CustomerSearchDto
					{
						Id = entity.Id,
						Name = entity.CompanyName,
						Classification = entity.Classification
					})
					.ToList();

				return dtos.ToIEnumerableResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, _scopedSettings, $"Entity {typeof(Customer).Name} could not be read.", ex, additional: new
				{
					SqlQuery = query.Query
				});

				return ResponseData<IEnumerable<DTOS_SEARCH.CustomerSearchDto>>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}

		public async Task<ResponseData<int>> SearchCountAsync(FilterRequestDto<DTOS_SEARCH.CustomerSearchDto> searchRequest)
		{
			var query = new QueryWrapper();
			try
			{
				var dbFilterRequest = TransformFilterRequest<DTOS_SEARCH.CustomerSearchDto, Customer>(searchRequest);
				dbFilterRequest = AddStatusQueryConditions(dbFilterRequest);
				query.Query = $"""

				SELECT
					 COUNT({CUSTOMER}.{nameof(Customer.Id)})
				FROM
					{TypeAnalyzer.GetTableName<Customer>()} {CUSTOMER}
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
				var messageGuid = Logger.LogError(this, _scopedSettings, $"Entity {typeof(Customer).Name}  could not be read", ex, additional: new
				{
					SqlQuery = query.Query
				});

				return ResponseData<int>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}
	}
}