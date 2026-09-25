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
	public class TimeOnlyHandlerTest
	{
		private readonly TimeOnlyHandler _handler = new TimeOnlyHandler();

		[TestMethod]
		public void SetValueOnSqlParameterIsTimeTest()
		{
			// Arrange
			var parameter = new SqlParameter();

			// Act
			_handler.SetValue(parameter, new TimeOnly(10, 30));

			// Assert
			parameter.SqlDbType.Should().Be(SqlDbType.Time);
			parameter.Value.Should().Be(new TimeSpan(10, 30, 0));
		}

		[TestMethod]
		public void SetValueDoesNotRequireASqlParameterTest()
		{
			// Arrange
			var parameter = A.Fake<IDbDataParameter>();

			// Act
			_handler.SetValue(parameter, new TimeOnly(10, 30));

			// Assert
			parameter.DbType.Should().Be(DbType.Time);
		}

		[TestMethod]
		public void ParseAcceptsTimeSpanAndTimeOnlyTest()
		{
			// Act
			var fromSqlServer = _handler.Parse(new TimeSpan(10, 30, 0));
			var fromNpgsql = _handler.Parse(new TimeOnly(10, 30));
			var fromSomethingElse = _handler.Parse("10:30");

			// Assert
			fromSqlServer.Should().Be(new TimeOnly(10, 30));
			fromNpgsql.Should().Be(new TimeOnly(10, 30));
			fromSomethingElse.Should().Be(default);
		}
	}
}