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
    internal class ForeignKeyColumnTypeMismatchRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table parent_table_with_int_key_column_1 ( column_1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table child_table_with_int_key_column_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references parent_table_with_int_key_column_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table child_table_with_text_key_column_1 (
    column_1 integer,
    column_2 text,
    constraint test_valid_fk foreign key (column_2) references parent_table_with_int_key_column_1 (column_1)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table parent_table_with_int_key_column_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table child_table_with_int_key_column_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table child_table_with_text_key_column_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyColumnTypeMismatchRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithMatchingTypesInForeignKeys_ProducesNoMessages()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("parent_table_with_int_key_column_1"), database.GetTable("child_table_with_int_key_column_1") };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithMismatchingTypesInForeignKeys_ProducesNoMessages()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("parent_table_with_int_key_column_1"), database.GetTable("child_table_with_text_key_column_1") };

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
