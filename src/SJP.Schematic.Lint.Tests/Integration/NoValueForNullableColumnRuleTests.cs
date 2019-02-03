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
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            IDbConnection connection = null;
            var dialect = Mock.Of<IDatabaseDialect>();
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new NoValueForNullableColumnRule(connection, dialect, level));
        }

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            IDatabaseDialect dialect = null;
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new NoValueForNullableColumnRule(connection, dialect, level));
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new NoValueForNullableColumnRule(connection, dialect, level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var rule = new NoValueForNullableColumnRule(connection, dialect, RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            var rule = new NoValueForNullableColumnRule(connection, dialect, RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithoutNullableColumns_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("table_without_nullable_columns_1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithoutNullableColumns_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("table_without_nullable_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsButNoRows_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("table_for_nullable_columns_1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithNullableColumnsButNoRows_ProducesNoMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("table_for_nullable_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsWithNoData_ProducesMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("table_for_nullable_columns_2").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithNullableColumnsWithNoData_ProducesMessages()
        {
            var rule = new NoValueForNullableColumnRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("table_for_nullable_columns_2").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }
    }
}
