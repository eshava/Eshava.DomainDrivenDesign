using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public abstract class AbstractDeactivateUseCase<TDomain, TIdentifier> : AbstractDomainModelUseCase
		where TDomain : AbstractEntity<TDomain, TIdentifier>
		where TIdentifier : struct
	{
		protected virtual Task<ResponseData<bool>> IsDeletableAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExecuteBeforeAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExecuteAfterAsync(TDomain entity)
		{
			return true.ToResponseDataAsync();
		}
	}
}