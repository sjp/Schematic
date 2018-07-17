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
    internal class ForeignKeyIsPrimaryKeyRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
create table parent_table_with_different_column_to_pk_column_1 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references parent_table_with_different_column_to_pk_column_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table parent_table_with_pk_column_to_pk_column_1 (
    column_1 integer not null primary key autoincrement,
    constraint test_fk_1 foreign key (column_1) references parent_table_with_pk_column_to_pk_column_1 (column_1)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table parent_table_with_different_column_to_pk_column_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table parent_table_with_pk_column_to_pk_column_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyIsPrimaryKeyRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithDifferentColumnToPrimaryKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("parent_table_with_different_column_to_pk_column_1") };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithSameColumnToPrimaryKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("parent_table_with_pk_column_to_pk_column_1") };

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
