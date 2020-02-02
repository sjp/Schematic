using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class RedundantIndexesRuleTests : SqliteTest
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
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new RedundantIndexesRule(level), Throws.ArgumentException);
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_2").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public async Task AnalyseTables_GivenTablesWithOnlyTablesWithRedundantIndexes_ProducesMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_3").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }
    }
}
