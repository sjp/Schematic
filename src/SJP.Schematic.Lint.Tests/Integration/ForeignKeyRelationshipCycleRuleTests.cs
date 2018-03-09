using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Lint.Tests.Integration
{
    [TestFixture]
    internal class ForeignKeyRelationshipCycleRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("pragma foreign_keys = OFF").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table cycle_table_1 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_2 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table cycle_table_2 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_3 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table cycle_table_3 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_1 (column_1)
)").ConfigureAwait(false);

            await Connection.ExecuteAsync("create table no_cycle_table_1 ( column_1 integer not null primary key autoincrement )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table no_cycle_table_2 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_cycle_table_1 (column_1)
)").ConfigureAwait(false);

            await Connection.ExecuteAsync("pragma foreign_keys = ON").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table cycle_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table cycle_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table cycle_table_3").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyRelationshipCycleRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithNoRelationshipCycle_ProducesNoMessages()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[]
            {
                database.GetTable("no_cycle_table_1"),
                database.GetTable("no_cycle_table_2")
            };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithRelationshipCycle_ProducesMessages()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = new SqliteRelationalDatabase(Dialect, Connection);

            fakeDatabase.Tables = new[]
            {
                database.GetTable("cycle_table_1"),
                database.GetTable("cycle_table_2"),
                database.GetTable("cycle_table_3")
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
