using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyIsPrimaryKeyRuleTests : SqliteTest
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
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithDifferentColumnToPrimaryKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("parent_table_with_different_column_to_pk_column_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithSameColumnToPrimaryKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("parent_table_with_pk_column_to_pk_column_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }
    }
}
