using System.Threading.Tasks;
using System.Transactions;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories;

namespace Eshava.DomainDrivenDesign.Infrastructure.Providers
{
	public abstract class AbstractInfrastructureProvider<TDomain, TIdentifier>
		where TDomain : AbstractEntity<TDomain, TIdentifier>
		where TIdentifier : struct
	{
		private readonly IDatabaseSettings _databaseSettings;

		protected AbstractInfrastructureProvider(
			IDatabaseSettings databaseSettings,
			IAbstractDomainModelRepository<TDomain, TIdentifier> repository
		)
		{
			_databaseSettings = databaseSettings;
			Repository = repository;
		}

		protected IAbstractDomainModelRepository<TDomain, TIdentifier> Repository { get; }

		public TransactionScope CreateTransactionScope(TransactionScopeAsyncFlowOption option = TransactionScopeAsyncFlowOption.Enabled)
		{
			return _databaseSettings.CreateTransactionScope(option);
		}

		public virtual Task<ResponseData<TDomain>> ReadAsync(TIdentifier entityId)
		{
			return Repository.ReadAsync(entityId);
		}

		public virtual async Task<ResponseData<TDomain>> SaveAsync(TDomain entity)
		{
			var isStorableResult = IsStorable(entity);
			if (isStorableResult.IsFaulty)
			{
				return isStorableResult.ConvertTo<TDomain>();
			}

			if (entity.Id.HasValue)
			{
				if (entity.Status == DomainDrivenDesign.Domain.Enums.Status.Inactive)
				{
					var deleteResult = await DeleteAsync(entity);
					if (deleteResult.IsFaulty)
					{
						return deleteResult.ConvertTo<TDomain>();
					}

					return entity.ToResponseData();
				}

				var updateResult = await UpdateAsync(entity);
				if (updateResult.IsFaulty)
				{
					return updateResult.ConvertTo<TDomain>();
				}

				return entity.ToResponseData();
			}

			var createResult = await CreateAsync(entity);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<TDomain>();
			}

			return entity.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> CreateAsync(TDomain entity)
		{
			var createPrerequisitesResult = await ExcecutePrerequisitesActionsForCreateAsync(entity);
			if (createPrerequisitesResult.IsFaulty)
			{
				return createPrerequisitesResult;
			}

			var createResult = await Repository.CreateAsync(entity);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<bool>();
			}

			var identifierResult = entity.SetIdentifier(createResult.Data);
			if (identifierResult.IsFaulty)
			{
				return identifierResult;
			}

			var createCompletionResult = await ExcecuteCompletionActionsForCreateAsync(entity);
			if (createCompletionResult.IsFaulty)
			{
				return createCompletionResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> UpdateAsync(TDomain entity)
		{
			var updatePrerequisitesResult = await ExcecutePrerequisitesActionsForUpdateAsync(entity);
			if (updatePrerequisitesResult.IsFaulty)
			{
				return updatePrerequisitesResult;
			}

			var updateResult = await Repository.UpdateAsync(entity);
			if (updateResult.IsFaulty)
			{
				return updateResult;
			}

			var updateCompletionResult = await ExcecuteCompletionActionsForUpdateAsync(entity);
			if (updateCompletionResult.IsFaulty)
			{
				return updateCompletionResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}

		protected virtual async Task<ResponseData<bool>> DeleteAsync(TDomain entity)
		{
			var updatePrerequisitesResult = await ExcecutePrerequisitesActionsForDeleteAsync(entity);
			if (updatePrerequisitesResult.IsFaulty)
			{
				return updatePrerequisitesResult;
			}

			var deleteResult = await Repository.DeleteAsync(entity);
			if (deleteResult.IsFaulty)
			{
				return deleteResult;
			}

			var deleteCompletionResult = await ExcecuteCompletionActionsForDeleteAsync(entity);
			if (deleteCompletionResult.IsFaulty)
			{
				return deleteCompletionResult;
			}

			entity.SetUnchanged();

			return true.ToResponseData();
		}

		protected ResponseData<bool> IsStorable(TDomain entity)
		{
			if (!entity.IsValid)
			{
				return MessageConstants.INVALIDDATAERROR.ToFaultyResponse<bool>();
			}

			if (!entity.IsChanged)
			{
				return MessageConstants.NOCHANGESERROR.ToFaultyResponse<bool>();
			}

			return true.ToResponseData();
		}

		protected virtual Task<ResponseData<bool>> ExcecutePrerequisitesActionsForCreateAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExcecuteCompletionActionsForCreateAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExcecutePrerequisitesActionsForUpdateAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExcecuteCompletionActionsForUpdateAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExcecutePrerequisitesActionsForDeleteAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExcecuteCompletionActionsForDeleteAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}
	}
}