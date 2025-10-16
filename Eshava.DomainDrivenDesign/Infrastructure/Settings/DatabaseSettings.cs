using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Transactions;

namespace Eshava.DomainDrivenDesign.Infrastructure.Settings
{
	public class DatabaseSettings : IDatabaseSettings
	{
		public DatabaseSettings(string connectionString)
		{
			ConnectionString = connectionString;
		}

		protected string ConnectionString { get; set; }

		public IDbConnection GetConnection()
		{
			if (ConnectionString.IsNullOrEmpty())
			{
				throw new ArgumentNullException(nameof(ConnectionString));
			}

			var connection = new SqlConnection(ConnectionString);

			return connection;
		}

		public TransactionScope CreateTransactionScope(TransactionScopeAsyncFlowOption option = TransactionScopeAsyncFlowOption.Enabled)
		{
			return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted, Timeout = new TimeSpan(0, 10, 0) }, option);
		}
	}
}