using System;
using System.Data;
using Eshava.DomainDrivenDesign.Infrastructure.Settings;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.DomainDrivenDesign.Infrastructure.Settings
{
	[TestClass, TestCategory("Infrastructure")]
	public class DatabaseSettingsTest
	{
		[TestMethod]
		public void ConnectionIsSqlConnectionByDefaultTest()
		{
			// Arrange
			var databaseSettings = new DatabaseSettings("Server=localhost;Database=Example");

			// Act
			using var connection = databaseSettings.GetConnection();

			// Assert
			connection.Should().BeOfType<SqlConnection>();
			connection.ConnectionString.Should().Contain("Database=Example");
		}

		[TestMethod]
		public void ConnectionCanBeCreatedByAnotherProviderTest()
		{
			// Arrange
			var databaseSettings = new OtherProviderDatabaseSettings("Host=localhost;Database=example");

			// Act
			var connection = databaseSettings.GetConnection();

			// Assert
			connection.Should().BeSameAs(databaseSettings.CreatedConnection);
			databaseSettings.ReceivedConnectionString.Should().Be("Host=localhost;Database=example");
		}

		[TestMethod]
		public void MissingConnectionStringIsRefusedTest()
		{
			// Arrange
			var databaseSettings = new OtherProviderDatabaseSettings(null);

			// Act
			var getConnection = () => databaseSettings.GetConnection();

			// Assert
			getConnection.Should().Throw<ArgumentNullException>();
			databaseSettings.ReceivedConnectionString.Should().BeNull();
		}

		private class OtherProviderDatabaseSettings : DatabaseSettings
		{
			public OtherProviderDatabaseSettings(string connectionString) : base(connectionString)
			{
			}

			public IDbConnection CreatedConnection { get; } = A.Fake<IDbConnection>();

			public string ReceivedConnectionString { get; private set; }

			protected override IDbConnection CreateConnection(string connectionString)
			{
				ReceivedConnectionString = connectionString;

				return CreatedConnection;
			}
		}
	}
}