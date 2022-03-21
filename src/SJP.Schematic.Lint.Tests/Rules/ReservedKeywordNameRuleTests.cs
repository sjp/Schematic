using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ReservedKeywordNameRuleTests
{
    [Test]
    public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
    {
        IDatabaseDialect dialect = null;
        const RuleLevel level = RuleLevel.Error;

        Assert.That(() => new ReservedKeywordNameRule(dialect, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var dialect = Mock.Of<IDatabaseDialect>();
        const RuleLevel level = (RuleLevel)999;

        Assert.That(() => new ReservedKeywordNameRule(dialect, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseSequences(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseSynonyms(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseRoutines(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularName_ProducesNoMessages()
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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularColumnNames_ProducesNoMessages()
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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessages()
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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithRegularName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var viewName = new Identifier("test");

        var view = new DatabaseView(
            viewName,
            "select 1",
            Array.Empty<IDatabaseColumn>()
        );
        var views = new[] { view };

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var viewName = new Identifier("SELECT");

        var view = new DatabaseView(
            viewName,
            "select 1",
            Array.Empty<IDatabaseColumn>()
        );
        var views = new[] { view };

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithRegularColumnNames_ProducesNoMessages()
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

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessages()
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

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithRegularName_ProducesNoMessages()
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

        var hasMessages = await rule.AnalyseSequences(sequences).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
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

        var hasMessages = await rule.AnalyseSequences(sequences).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithRegularName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var synonymName = new Identifier("test");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var hasMessages = await rule.AnalyseSynonyms(synonyms).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var synonymName = new Identifier("SELECT");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var hasMessages = await rule.AnalyseSynonyms(synonyms).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithRegularName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var routineName = new Identifier("test");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var hasMessages = await rule.AnalyseRoutines(routines).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var routineName = new Identifier("SELECT");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var hasMessages = await rule.AnalyseRoutines(routines).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    private static IDatabaseDialect CreateFakeDialect() => new FakeDatabaseDialect { ReservedKeywords = new[] { "SELECT" } };
}