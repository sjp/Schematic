using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Lint.Tests.Integration
{
    [TestFixture]
    internal class ForeignKeyIndexRuleTests : SqliteTest
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
            await Connection.ExecuteAsync("drop table not_indexed_child_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyIndexRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithTableWithIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[]
            {
                database.GetTable("no_index_parent_table_1"),
                database.GetTable("indexed_child_table_1")
            };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithTableWithoutIndexOnForeignKey_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[]
            {
                database.GetTable("no_index_parent_table_1"),
                database.GetTable("not_indexed_child_table_1")
            };

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
