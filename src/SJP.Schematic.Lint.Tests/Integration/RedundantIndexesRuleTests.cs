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
    internal class RedundantIndexesRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table valid_table_1 ( column_1 integer )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table valid_table_2 ( column_1 integer, column_2 integer, column_3 integer )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_valid_table_1 on valid_table_2 ( column_2, column_3 )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table valid_table_3 ( column_1 integer, column_2 integer, column_3 integer )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_valid_table_3_1 on valid_table_3 ( column_2 )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_valid_table_3_2 on valid_table_3 ( column_2, column_3 )").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table valid_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table valid_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table valid_table_3").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new RedundantIndexesRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithoutIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("valid_table_1") };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("valid_table_2") };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyTablesWithRedundantIndexes_ProducesMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[] { database.GetTable("valid_table_3") };

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
