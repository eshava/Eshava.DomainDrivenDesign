using System;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Linq.Interfaces;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Settings;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace Eshava.DomainDrivenDesign.Infrastructure.Repositories
{
	public abstract class AbstractChildDomainModelRepository<TDomain, TCreationBag, TData, TIdentifier, TScopedSettings>
		: AbstractEntityRepository<TDomain, TData, TIdentifier, TScopedSettings>
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TCreationBag : class
		where TData : AbstractDatabaseModel<TIdentifier>, new()
		where TIdentifier : struct
		where TScopedSettings : AbstractScopedSettings
	{

		public AbstractChildDomainModelRepository(
			IDatabaseSettings databaseSettings,
			TScopedSettings scopedSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger logger
		) : base(databaseSettings, scopedSettings, transformQueryEngine, logger)
		{

		}

		public virtual Task<ResponseData<TIdentifier>> CreateAsync(TDomain model, TCreationBag creationBag)
		{
			try
			{
				var newEntity = FromDomainModel(model, creationBag);

				return CreateAsync(newEntity);
			}
			catch (Exception ex)
			{
				var messageGuid = Logger.LogError(this, ScopedSettings, $"Error creating {typeof(TData).Name}", ex, additional: new
				{
					CreationBag = creationBag
				});

				return ResponseData<TIdentifier>.CreateInternalServerError(MessageConstants.CREATEDATAERROR, ex, messageGuid).ToTask();
			}
		}

		protected abstract TData FromDomainModel(TDomain model, TCreationBag creationBag);
		protected virtual TData FromDomainModel(TData data, TDomain model, TCreationBag creationBag)
		{
			return data;
		}
	}
}