using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyRelationshipCycleRuleTests : SqliteTest
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
create table cycle_table_3 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table cycle_table_4 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_1 (column_1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table cycle_table_2 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references cycle_table_3 (column_1)
    constraint test_fk_2 foreign key (column_2) references cycle_table_4 (column_1)
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
            await Connection.ExecuteAsync("drop table cycle_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table cycle_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table cycle_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table cycle_table_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ForeignKeyRelationshipCycleRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithNoRelationshipCycle_ProducesNoMessages()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("no_cycle_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("no_cycle_table_2").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithRelationshipCycle_ProducesMessages()
        {
            var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("cycle_table_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("cycle_table_2").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("cycle_table_3").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("cycle_table_4").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }
    }
}
