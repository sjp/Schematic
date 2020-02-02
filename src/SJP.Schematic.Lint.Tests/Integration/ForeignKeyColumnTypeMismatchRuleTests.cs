using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class ForeignKeyColumnTypeMismatchRuleTests : SqliteTest
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
            Assert.That(() => new ForeignKeyColumnTypeMismatchRule(level), Throws.ArgumentException);
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithMatchingTypesInForeignKeys_ProducesNoMessages()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("parent_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("child_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithMismatchingTypesInForeignKeys_ProducesMessages()
        {
            var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("parent_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetTable("child_table_with_text_key_column_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }
    }
}
