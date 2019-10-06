using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class WhitespaceNameRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new WhitespaceNameRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseViews(null));
        }

        [Test]
        public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSequences(null));
        }

        [Test]
        public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseSynonyms(null));
        }

        [Test]
        public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseRoutines(null));
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
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

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var tableName = new Identifier("   test      ");

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

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);

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

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithColumnNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "   test_column ",
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

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseViews_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn>()
            );
            var views = new[] { view };

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseViews_GivenViewWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("   test   ");

            var view = new DatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn>()
            );
            var views = new[] { view };

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseViews_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
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

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseViews_GivenViewWithColumnNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "   test_column   ",
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

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseSequences_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
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

            var hasMessages = await rule.AnalyseSequences(sequences).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseSequences_GivenSequenceWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("   test   ");

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

            var hasMessages = await rule.AnalyseSequences(sequences).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseSynonyms_GivenSynonymWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var synonymName = new Identifier("test");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var hasMessages = await rule.AnalyseSynonyms(synonyms).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var synonymName = new Identifier("   test   ");

            var synonym = new DatabaseSynonym(synonymName, "target");
            var synonyms = new[] { synonym };

            var hasMessages = await rule.AnalyseSynonyms(synonyms).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }

        [Test]
        public static async Task AnalyseRoutines_GivenRoutineWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var routineName = new Identifier("test");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var hasMessages = await rule.AnalyseRoutines(routines).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseRoutines_GivenRoutineWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var routineName = new Identifier("   test   ");

            var routine = new DatabaseRoutine(routineName, "routine_definition");
            var routines = new[] { routine };

            var hasMessages = await rule.AnalyseRoutines(routines).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }
    }
}
