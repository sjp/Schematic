using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class NoValueForNullableColumnRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table table_without_nullable_columns_1 ( column_1 integer not null )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_for_nullable_columns_1 ( column_1 integer not null, column_2 integer null )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_for_nullable_columns_2 ( column_1 integer not null, column_2 integer null )").ConfigureAwait(false);
            await Connection.ExecuteAsync("insert into table_for_nullable_columns_2 ( column_1 ) values (1)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table table_without_nullable_columns_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_for_nullable_columns_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_for_nullable_columns_2").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentNullException()
        {
            IDbConnection connection = null;
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new NoValueForNullableColumnRule(connection, level));
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumenException()
        {
            var connection = Mock.Of<IDbConnection>();
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new NoValueForNullableColumnRule(connection, level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var rule = new NoValueForNullableColumnRule(connection, RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithoutNullableColumns_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("table_without_nullable_columns_1").UnwrapSome() };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithNullableColumnsButNoRows_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("table_for_nullable_columns_1").UnwrapSome() };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithNullableColumnsWithNoData_ProducesMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("table_for_nullable_columns_2").UnwrapSome() };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
