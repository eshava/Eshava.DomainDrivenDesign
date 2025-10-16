using System.Threading.Tasks;
using System.Transactions;
using Eshava.Core.Models;

namespace Eshava.DomainDrivenDesign.Application.Interfaces.Providers
{
	public interface IInfrastructureProvider<TDomain, TIdentifier> 
		where TDomain : class
		where TIdentifier : struct
	{
		TransactionScope CreateTransactionScope(TransactionScopeAsyncFlowOption option = TransactionScopeAsyncFlowOption.Enabled);
		Task<ResponseData<TDomain>> ReadAsync(TIdentifier entityId);
		Task<ResponseData<TDomain>> SaveAsync(TDomain entity);
	}
}