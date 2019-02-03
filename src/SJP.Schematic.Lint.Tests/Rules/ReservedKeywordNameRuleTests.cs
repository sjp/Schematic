using System;
using System.Collections.Generic;
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
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            IDatabaseDialect dialect = null;
            const RuleLevel level = RuleLevel.Error;

            Assert.Throws<ArgumentNullException>(() => new ReservedKeywordNameRule(dialect, level));
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            const RuleLevel level = (RuleLevel)999;

            Assert.Throws<ArgumentException>(() => new ReservedKeywordNameRule(dialect, level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseTablesAsync_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTablesAsync(null));
        }

        [Test]
        public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseViews(null));
        }

        [Test]
        public static void AnalyseViewsAsync_GivenNullViews_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseViewsAsync(null));
        }

        [Test]
        public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSequences(null));
        }

        [Test]
        public static void AnalyseSequencesAsync_GivenNullSequences_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSequencesAsync(null));
        }

        [Test]
        public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSynonyms(null));
        }

        [Test]
        public static void AnalyseSynonymsAsync_GivenNullSynonyms_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSynonymsAsync(null));
        }

        [Test]
        public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseRoutines(null));
        }

        [Test]
        public static void AnalyseRoutinesAsync_GivenNullRoutines_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseRoutinesAsync(null));
        }

        [Test]
        public static void AnalyseTables_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var tableName = new Identifier("test");

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
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var tableName = new Identifier("test");

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
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var tableName = new Identifier("SELECT");

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
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var tableName = new Identifier("SELECT");

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
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseTables_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
            var tables = new[] { table };

            var messages = rule.AnalyseTables(tables);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseTablesAsync_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
            var tables = new[] { table };

            var messages = await rule.AnalyseTablesAsync(tables).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseViews_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            var views = new[] { view };

            var messages = rule.AnalyseViews(views);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseViewsAsync_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            var views = new[] { view };

            var messages = await rule.AnalyseViewsAsync(views).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseViews_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("SELECT");

            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            var views = new[] { view };

            var messages = rule.AnalyseViews(views);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseViewsAsync_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("SELECT");

            var view = new DatabaseView(
                viewName,
                "select 1",
                Array.Empty<IDatabaseColumn>()
            );
            var views = new[] { view };

            var messages = await rule.AnalyseViewsAsync(views).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseViews_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            var views = new[] { view };

            var messages = rule.AnalyseViews(views);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseViewsAsync_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            var views = new[] { view };

            var messages = await rule.AnalyseViewsAsync(views).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseViews_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "SELECT",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            var views = new[] { view };

            var messages = rule.AnalyseViews(views);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseViewsAsync_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "SELECT",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn }
            );
            var views = new[] { view };

            var messages = await rule.AnalyseViewsAsync(views).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseSequences_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var sequenceName = new Identifier("test");

            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            var sequences = new[] { sequence };

            var messages = rule.AnalyseSequences(sequences);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseSequencesAsync_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var sequenceName = new Identifier("test");

            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            var sequences = new[] { sequence };

            var messages = await rule.AnalyseSequencesAsync(sequences).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseSequences_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var sequenceName = new Identifier("SELECT");

            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            var sequences = new[] { sequence };

            var messages = rule.AnalyseSequences(sequences);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseSequencesAsync_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var sequenceName = new Identifier("SELECT");

            var sequence = new DatabaseSequence(
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            var sequences = new[] { sequence };

            var messages = await rule.AnalyseSequencesAsync(sequences).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseSynonyms_GivenSynonymWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var synonymName = new Identifier("test");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var messages = rule.AnalyseSynonyms(synonyms);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseSynonymsAsync_GivenSynonymWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var synonymName = new Identifier("test");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var messages = await rule.AnalyseSynonymsAsync(synonyms).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseSynonyms_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var synonymName = new Identifier("SELECT");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var messages = rule.AnalyseSynonyms(synonyms);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseSynonymsAsync_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var synonymName = new Identifier("SELECT");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var messages = await rule.AnalyseSynonymsAsync(synonyms).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseRoutines_GivenRoutineWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var routineName = new Identifier("test");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var messages = rule.AnalyseRoutines(routines);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseRoutinesAsync_GivenRoutineWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var routineName = new Identifier("test");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var messages = await rule.AnalyseRoutinesAsync(routines).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseRoutines_GivenRoutineWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var routineName = new Identifier("SELECT");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var messages = rule.AnalyseRoutines(routines);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static async Task AnalyseRoutinesAsync_GivenRoutineWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
            var routineName = new Identifier("SELECT");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var messages = await rule.AnalyseRoutinesAsync(routines).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        private static IDatabaseDialect CreateFakeDialect() => new FakeDatabaseDialect { ReservedKeywords = new[] { "SELECT" } };
    }
}
