using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.Settings;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Settings;
using Eshava.Storm;
using Microsoft.Extensions.Logging;

namespace Eshava.DomainDrivenDesign.Infrastructure.Repositories
{
	public abstract class AbstractEntityRepository<TDomain, TData, TIdentifier, TScopedSettings> : AbstractRepository
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TData : AbstractDatabaseModel<TIdentifier>, new()
		where TIdentifier : struct
		where TScopedSettings : AbstractScopedSettings
	{
		protected static Dictionary<string, Func<object, object>> PropertyValueToDomainMappings { get; set; } = [];
		protected static Dictionary<string, Func<object, object>> PropertyValueToDataMappings { get; set; } = [];

		public AbstractEntityRepository(
			IDatabaseSettings databaseSettings,
			TScopedSettings scopedSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger logger
		) : base(transformQueryEngine)
		{
			DatabaseSettings = databaseSettings;
			ScopedSettings = scopedSettings;
			Logger = logger;
		}

		protected virtual bool EnableSoftDelete { get; } = CommonSettings.EnableSoftDelete;
		protected IDatabaseSettings DatabaseSettings { get; }
		protected ILogger Logger { get; }
		protected TScopedSettings ScopedSettings { get; }

		public virtual async Task<ResponseData<bool>> DeleteAsync(TDomain model)
		{
			if (EnableSoftDelete)
			{
				return await UpdateAsync(model);
			}

			try
			{
				var entityId = model.Id.Value;

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.DeleteAsync(new TData { Id = entityId });
					if (!result)
					{
						return ResponseData<bool>.CreateFaultyResponse(MessageConstants.DELETEDATAERROR);
					}

					return true.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, $"Error deleting {typeof(TData).Name}", ex, additional: new
				{
					EntityId = model.Id.Value
				});

				return ResponseData<bool>.CreateInternalServerError(MessageConstants.UPDATEDATAERROR, ex, messageGuid);
			}
		}

		public virtual async Task<ResponseData<bool>> UpdateAsync(TDomain model)
		{
			try
			{
				var entityId = model.Id.Value;
				var patches = model.GetChanges();

				var changes = new Dictionary<string, object>
				{
					{ nameof(AbstractDatabaseModel<TIdentifier>.Id), entityId }
				};

				AdjustDatabaseModelForPatch(changes);

				foreach (var patch in patches)
				{
					var dataProperty = GetPropertyName(patch);
					if (!changes.ContainsKey(dataProperty))
					{
						changes.Add(dataProperty, MapToDataPropertyValue(dataProperty, patch.Value));
					}
				}

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.UpdatePatchAsync<TData>(changes);
					if (!result)
					{
						return ResponseData<bool>.CreateFaultyResponse(MessageConstants.UPDATEDATAERROR);
					}

					return true.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, $"Error patching {typeof(TData).Name}", ex, additional: new
				{
					EntityId = model.Id.Value
				});

				return ResponseData<bool>.CreateInternalServerError(MessageConstants.UPDATEDATAERROR, ex, messageGuid);
			}
		}

		protected virtual void AdjustDatabaseModelForCreate(TData data)
		{

		}

		protected virtual void AdjustDatabaseModelForPatch(IDictionary<string, object> changes)
		{

		}

		protected virtual async Task<ResponseData<TIdentifier>> CreateAsync(TData newEntity)
		{
			try
			{
				AdjustDatabaseModelForCreate(newEntity);

				using (var connection = DatabaseSettings.GetConnection())
				{
					var result = await connection.InsertAsync<TData, TIdentifier>(newEntity);

					return result.ToResponseData();
				}
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, $"Error creating {typeof(TData).Name}", ex);

				return ResponseData<TIdentifier>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex, messageGuid);
			}
		}

		protected object MapToDataPropertyValue(string dataPropertyName, object value)
		{
			if (PropertyValueToDataMappings.ContainsKey(dataPropertyName))
			{
				return PropertyValueToDataMappings[dataPropertyName](value);
			}

			return value;
		}

		protected virtual string GetPropertyName(Patch<TDomain> patch)
		{
			//var sourceMemberExpression = patch.Property.Body.GetMemberExpression();
			//var targetMemberExpression = TransformQueryEngine.TransformMemberExpression<TDomain, TData>(sourceMemberExpression);

			//return targetMemberExpression.Member.Member.Name;

			return patch.PropertyName;
		}

		protected FilterRequestDto<TData> TransformFilterRequest(FilterRequestDto<TDomain> sourceFilterRequest)
		{
			return TransformFilterRequest<TDomain, TData>(sourceFilterRequest);
		}
	}
}