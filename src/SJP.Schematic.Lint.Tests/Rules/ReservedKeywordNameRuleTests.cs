using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class ReservedKeywordNameRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ReservedKeywordNameRule(level));
        }

        [Test]
        public static void AnalyseDatabaseAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabaseAsync(null));
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var tableName = new Identifier("test");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                tableName,
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
        public static async Task AnalyseDatabaseAsync_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var tableName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                tableName,
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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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
        public static async Task AnalyseDatabaseAsync_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "SELECT",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var database = CreateFakeDatabase();
            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            database.Views = new[] { view };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            database.Views = new[] { view };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            database.Views = new[] { view };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "SELECT",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            database.Views = new[] { view };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("test");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            database.Sequences = new[] { sequence };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            database.Sequences = new[] { sequence };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenSynonymWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var synonymName = new Identifier("test");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var synonymName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenRoutineWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var routineName = new Identifier("test");

            var database = CreateFakeDatabase();
            var routine = new DatabaseRoutine(routineName, "routine_definition");
            database.Routines = new[] { routine };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenRoutineWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var routineName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var routine = new DatabaseRoutine(routineName, "routine_definition");
            database.Routines = new[] { routine };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect { ReservedKeywords = new[] { "SELECT" } };
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
