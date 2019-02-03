using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyIndexRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table no_index_parent_table_1 ( column_1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_indexed_child_table_1 on indexed_child_table_1 (column_2)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table indexed_child_table_2 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_indexed_child_table_2 on indexed_child_table_2 (column_2, column_1)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table not_indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table no_index_parent_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table indexed_child_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table indexed_child_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table not_indexed_child_table_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyIndexRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public void AnalyseTables_GivenTablesWithTableWithIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("indexed_child_table_1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithTableWithIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenTablesWithTableWithoutIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().GetAwaiter().GetResult(),
                database.GetTable("not_indexed_child_table_1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithTableWithoutIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("not_indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }
    }
}
