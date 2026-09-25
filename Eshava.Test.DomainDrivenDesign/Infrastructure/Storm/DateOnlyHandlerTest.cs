using System;
using System.Data;
using Eshava.DomainDrivenDesign.Infrastructure.Storm;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.DomainDrivenDesign.Infrastructure.Storm
{
	[TestClass, TestCategory("Infrastructure")]
	public class DateOnlyHandlerTest
	{
		private readonly DateOnlyHandler _handler = new DateOnlyHandler();

		[TestMethod]
		public void SetValueOnSqlParameterIsDateTest()
		{
			// Arrange
			var parameter = new SqlParameter();

			// Act
			_handler.SetValue(parameter, new DateOnly(2026, 9, 25));

			// Assert
			parameter.SqlDbType.Should().Be(SqlDbType.Date);
			parameter.Value.Should().Be(new DateTime(2026, 9, 25));
		}

		[TestMethod]
		public void SetValueDoesNotRequireASqlParameterTest()
		{
			// Arrange
			var parameter = A.Fake<IDbDataParameter>();

			// Act
			_handler.SetValue(parameter, new DateOnly(2026, 9, 25));

			// Assert
			parameter.DbType.Should().Be(DbType.Date);
		}

		[TestMethod]
		public void ParseAcceptsDateTimeAndDateOnlyTest()
		{
			// Act
			var fromSqlServer = _handler.Parse(new DateTime(2026, 9, 25));
			var fromNpgsql = _handler.Parse(new DateOnly(2026, 9, 25));
			var fromSomethingElse = _handler.Parse("2026-09-25");

			// Assert
			fromSqlServer.Should().Be(new DateOnly(2026, 9, 25));
			fromNpgsql.Should().Be(new DateOnly(2026, 9, 25));
			fromSomethingElse.Should().Be(default);
		}
	}
}