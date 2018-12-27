using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class DisabledObjectsRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new DisabledObjectsRule(level));
        }

        [Test]
        public static void AnalyseDatabaseAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabaseAsync(null));
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithNoDisabledObjects_ProducesNoMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledPrimaryKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn },
                false
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledForeignKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testForeignKey = new DatabaseKey(
                Option<Identifier>.Some("test_foreign_key"),
                DatabaseKeyType.Foreign,
                new[] { testColumn },
                false
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn },
                false
            );
            var testRelationalKey = new DatabaseRelationalKey(
                "child_table",
                testForeignKey,
                "parent_table",
                testPrimaryKey,
                System.Data.Rule.Cascade,
                System.Data.Rule.Cascade
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                new[] { testRelationalKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledUniqueKey_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Option<Identifier>.Some("test_unique_key"),
                DatabaseKeyType.Unique,
                new[] { testColumn },
                false
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                new[] { testUniqueKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledIndex_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testIndex = new DatabaseIndex(
                "test_index",
                true,
                new[] { new DatabaseIndexColumn("test_column", testColumn, IndexColumnOrder.Ascending) },
                Array.Empty<IDatabaseColumn>(),
                false
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { testIndex },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledCheck_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testCheck = new DatabaseCheckConstraint(
                Option<Identifier>.Some("test_check"),
                "test_check_definition",
                false
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                new[] { testCheck },
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithDisabledTrigger_ProducesMessages()
        {
            var rule = new DisabledObjectsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testTrigger = new DatabaseTrigger(
                "test_check",
                "test_check_definition",
                TriggerQueryTiming.After,
                TriggerEvent.Insert,
                false
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                new[] { testTrigger }
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
