using System;
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
            Assert.Throws<ArgumentException>(() => new RedundantIndexesRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithoutIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("valid_table_1").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithoutIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("valid_table_2").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_2").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseTables_GivenTablesWithOnlyTablesWithRedundantIndexes_ProducesMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                database.GetTable("valid_table_3").UnwrapSomeAsync().GetAwaiter().GetResult()
            };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public async Task AnalyseTablesAsync_GivenTablesWithOnlyTablesWithRedundantIndexes_ProducesMessages()
        {
            var rule = new RedundantIndexesRule(RuleLevel.Error);
            var database = GetSqliteDatabase();

            var tables = new[]
            {
                await database.GetTable("valid_table_3").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }
    }
}
