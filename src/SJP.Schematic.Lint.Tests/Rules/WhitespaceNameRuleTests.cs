using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class WhitespaceNameRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new WhitespaceNameRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseSequences(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseSynonyms(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseRoutines(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var tableName = new Identifier("test");

        var table = new RelationalDatabaseTable(
            tableName,
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var tableName = new Identifier("   test      ");

        var table = new RelationalDatabaseTable(
            tableName,
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
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
            [testColumn],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
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
            [testColumn],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var viewName = new Identifier("test");

        var view = new DatabaseView(
            viewName,
            "select 1",
            []
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var viewName = new Identifier("   test   ");

        var view = new DatabaseView(
            viewName,
            "select 1",
            []
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
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
            [testColumn]
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
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
            [testColumn]
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var sequenceName = new Identifier("test");

        var sequence = new DatabaseSequence(
            sequenceName,
            TestDbTypes.BigInteger,
            1,
            1,
            1,
            100,
            true,
            SequenceCacheMode.Sized,
            Option<int>.Some(10),
            true
        );
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var sequenceName = new Identifier("   test   ");

        var sequence = new DatabaseSequence(
            sequenceName,
            TestDbTypes.BigInteger,
            1,
            1,
            1,
            100,
            true,
            SequenceCacheMode.Sized,
            Option<int>.Some(10),
            true
        );
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var synonymName = new Identifier("test");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var synonymName = new Identifier("   test   ");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var routineName = new Identifier("test");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var routineName = new Identifier("   test   ");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularIndexName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testIndex = new DatabaseIndex(
            "test_index",
            false,
            [new DatabaseIndexColumn("test_column", testColumn, IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            null,
            [],
            [],
            [],
            [testIndex],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithIndexNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testIndex = new DatabaseIndex(
            "test name",
            false,
            [new DatabaseIndexColumn("test_column", testColumn, IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            null,
            [],
            [],
            [],
            [testIndex],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("test name"),
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            primaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnnamedPrimaryKey_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var primaryKey = new DatabaseKey(
            Option<Identifier>.None,
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            primaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUniqueKeyNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var uniqueKey = new DatabaseKey(
            Option<Identifier>.Some("test name"),
            DatabaseKeyType.Unique,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            null,
            [uniqueKey],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithForeignKeyNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test name"),
            DatabaseKeyType.Foreign,
            [testColumn],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("parent_pk"),
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            "test",
            childKey,
            "parent_table",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            null,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithCheckConstraintNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testCheck = new DatabaseCheckConstraint(
            Option<Identifier>.Some("test name"),
            "test_check_definition",
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [testCheck],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUnnamedCheckConstraint_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testCheck = new DatabaseCheckConstraint(
            Option<Identifier>.None,
            "test_check_definition",
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [testCheck],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithTriggerNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var testTrigger = new DatabaseTrigger(
            "test name",
            "test_trigger_definition",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            [testTrigger]
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseTables_GivenTablesWithSchemaNameContainingWhitespace_ProducesOneMessageForTheSchema()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var firstTable = new RelationalDatabaseTable(
            Identifier.CreateQualifiedIdentifier("test name", "first_table"),
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var secondTable = new RelationalDatabaseTable(
            Identifier.CreateQualifiedIdentifier("test name", "second_table"),
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseViews_GivenViewsWithSchemaNameContainingWhitespace_ProducesOneMessageForTheSchema()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var firstView = new DatabaseView(Identifier.CreateQualifiedIdentifier("test name", "first_view"), "select 1", []);
        var secondView = new DatabaseView(Identifier.CreateQualifiedIdentifier("test name", "second_view"), "select 1", []);
        var views = new[] { firstView, secondView };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithSchemaNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var sequence = new DatabaseSequence(
            Identifier.CreateQualifiedIdentifier("test name", "test_sequence"),
            TestDbTypes.BigInteger,
            1,
            1,
            1,
            100,
            true,
            SequenceCacheMode.Sized,
            Option<int>.Some(10),
            true
        );
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithSchemaNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var synonym = new DatabaseSynonym(Identifier.CreateQualifiedIdentifier("test name", "test_synonym"), "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithSchemaNameContainingWhitespace_ProducesMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var routine = new DatabaseRoutine(Identifier.CreateQualifiedIdentifier("test name", "test_routine"), "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Has.Exactly(1).Items);
    }
}
