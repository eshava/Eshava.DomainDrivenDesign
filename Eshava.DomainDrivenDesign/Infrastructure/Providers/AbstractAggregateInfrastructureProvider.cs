using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories;

namespace Eshava.DomainDrivenDesign.Infrastructure.Providers
{
	public abstract class AbstractAggregateInfrastructureProvider<TDomain, TIdentifier> : AbstractInfrastructureProvider<TDomain, TIdentifier>
		where TDomain : AbstractAggregate<TDomain, TIdentifier>
		where TIdentifier : struct
	{
		private readonly IDatabaseSettings _databaseSettings;

		protected AbstractAggregateInfrastructureProvider(
			IDatabaseSettings databaseSettings,
			IAbstractDomainModelRepository<TDomain, TIdentifier> repository
		) : base(databaseSettings, repository)
		{
			_databaseSettings = databaseSettings;
		}

		public virtual bool UseTransaction { get; } = true;

		public override async Task<ResponseData<TDomain>> SaveAsync(TDomain entity)
		{
			if (UseTransaction)
			{
				using (var transaction = _databaseSettings.CreateTransactionScope())
				{
					var saveResult = await base.SaveAsync(entity);
					if (saveResult.IsFaulty)
					{
						return saveResult;
					}

					transaction.Complete();

					return saveResult;
				}
			}

			return await base.SaveAsync(entity);
		}

		protected virtual async Task<ResponseData<bool>> SaveChildsAsync<TChildDomain, TCreationBag, TVIdentifier>(IEnumerable<TChildDomain> entities, TCreationBag creationBag, IAbstractChildDomainModelRepository<TChildDomain, TCreationBag, TVIdentifier> repository, Func<TChildDomain, Task<ResponseData<bool>>> isDeletableAsync)
			where TChildDomain : AbstractEntity<TChildDomain, TVIdentifier>
			where TCreationBag : class
			where TVIdentifier : struct
		{
			foreach (var entity in entities)
			{
				if (entity.Id.HasValue)
				{
					if (entity.Status == Domain.Enums.Status.Inactive)
					{
						var deleteResult = await DeleteChildAsync(entity, repository, isDeletableAsync);
						if (deleteResult.IsFaulty)
						{
							return deleteResult;
						}
					}
					else
					{
						var updateResult = await UpdateChildAsync(entity, repository);
						if (updateResult.IsFaulty)
						{
							return updateResult;
						}
					}
				}
				else
				{
					var createResult = await CreateChildAsync(entity, creationBag, repository);
					if (createResult.IsFaulty)
					{
						return createResult;
					}
				}
			}

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> CreateChildAsync<TChildDomain, TCreationBag, TVIdentifier>(TChildDomain entity, TCreationBag creationBag, IAbstractChildDomainModelRepository<TChildDomain, TCreationBag, TVIdentifier> repository)
			where TChildDomain : AbstractEntity<TChildDomain, TVIdentifier>
			where TCreationBag : class
			where TVIdentifier : struct
		{
			var createResult = await repository.CreateAsync(entity, creationBag);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<bool>();
			}

			var identifierResult = entity.SetIdentifier(createResult.Data);
			if (identifierResult.IsFaulty)
			{
				return identifierResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> UpdateChildAsync<TChildDomain, TCreationBag, TVIdentifier>(TChildDomain entity, IAbstractChildDomainModelRepository<TChildDomain, TCreationBag, TVIdentifier> repository)
			where TChildDomain : AbstractEntity<TChildDomain, TVIdentifier>
			where TCreationBag : class
			where TVIdentifier : struct
		{
			var updateResult = await repository.UpdateAsync(entity);
			if (updateResult.IsFaulty)
			{
				return updateResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> DeleteChildsAsync<TChildDomain, TCreationBag, TVIdentifier>(IEnumerable<TChildDomain> entities, IAbstractChildDomainModelRepository<TChildDomain, TCreationBag, TVIdentifier> repository, Func<TChildDomain, Task<ResponseData<bool>>> isDeletableAsync)
			where TChildDomain : AbstractEntity<TChildDomain, TVIdentifier>
			where TCreationBag : class
			where TVIdentifier : struct
		{
			foreach (var entity in entities)
			{
				var deleteResult = await DeleteChildAsync(entity, repository, isDeletableAsync);
				if (deleteResult.IsFaulty)
				{
					return deleteResult;
				}
			}

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> DeleteChildAsync<TChildDomain, TCreationBag, TVIdentifier>(TChildDomain entity, IAbstractChildDomainModelRepository<TChildDomain, TCreationBag, TVIdentifier> repository, Func<TChildDomain, Task<ResponseData<bool>>> isDeletableAsync)
			where TChildDomain : AbstractEntity<TChildDomain, TVIdentifier>
			where TCreationBag : class
			where TVIdentifier : struct
		{
			var isDeleteableResult = isDeletableAsync is null
				? true.ToResponseData()
				: await isDeletableAsync(entity);

			if (isDeleteableResult.IsFaulty)
			{
				return isDeleteableResult;
			}

			var deleteResult = await repository.DeleteAsync(entity);
			if (deleteResult.IsFaulty)
			{
				return deleteResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}
	}
}