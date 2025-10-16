using System.Data;
using System.Transactions;

namespace Eshava.DomainDrivenDesign.Infrastructure.Interfaces
{
	public interface IDatabaseSettings
	{
		IDbConnection GetConnection();
		TransactionScope CreateTransactionScope(TransactionScopeAsyncFlowOption option = TransactionScopeAsyncFlowOption.Enabled);
	}
}