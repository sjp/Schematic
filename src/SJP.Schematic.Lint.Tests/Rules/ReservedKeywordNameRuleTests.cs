using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;
using SJP.Schematic.Tests.Utilities;

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
    public static async Task AnalyseTables_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = new Identifier("SELECT");

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
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
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
    public static async Task AnalyseViews_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var viewName = new Identifier("SELECT");

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
            [testColumn]
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
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
            [testColumn]
        );
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithRegularName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
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
    public static async Task AnalyseSequences_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var sequenceName = new Identifier("SELECT");

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
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var synonymName = new Identifier("test");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var synonymName = new Identifier("SELECT");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithRegularName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var routineName = new Identifier("test");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var routineName = new Identifier("SELECT");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularIndexName_ProducesNoMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
    public static async Task AnalyseTables_GivenTableWithIndexNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testIndex = new DatabaseIndex(
            "SELECT",
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
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("SELECT"),
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
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
    public static async Task AnalyseTables_GivenTableWithUniqueKeyNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var uniqueKey = new DatabaseKey(
            Option<Identifier>.Some("SELECT"),
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
    public static async Task AnalyseTables_GivenTableWithForeignKeyNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("SELECT"),
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
    public static async Task AnalyseTables_GivenTableWithCheckConstraintNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testCheck = new DatabaseCheckConstraint(
            Option<Identifier>.Some("SELECT"),
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
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

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
    public static async Task AnalyseTables_GivenTableWithTriggerNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var testTrigger = new DatabaseTrigger(
            "SELECT",
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
    public static async Task AnalyseTables_GivenTablesWithSchemaNameContainingReservedKeyword_ProducesOneMessageForTheSchema()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var firstTable = new RelationalDatabaseTable(
            Identifier.CreateQualifiedIdentifier("SELECT", "first_table"),
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
            Identifier.CreateQualifiedIdentifier("SELECT", "second_table"),
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
    public static async Task AnalyseViews_GivenViewsWithSchemaNameContainingReservedKeyword_ProducesOneMessageForTheSchema()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var firstView = new DatabaseView(Identifier.CreateQualifiedIdentifier("SELECT", "first_view"), "select 1", []);
        var secondView = new DatabaseView(Identifier.CreateQualifiedIdentifier("SELECT", "second_view"), "select 1", []);
        var views = new[] { firstView, secondView };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithSchemaNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var sequence = new DatabaseSequence(
            Identifier.CreateQualifiedIdentifier("SELECT", "test_sequence"),
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
    public static async Task AnalyseSynonyms_GivenSynonymWithSchemaNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var synonym = new DatabaseSynonym(Identifier.CreateQualifiedIdentifier("SELECT", "test_synonym"), "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithSchemaNameContainingReservedKeyword_ProducesMessages()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);

        var routine = new DatabaseRoutine(Identifier.CreateQualifiedIdentifier("SELECT", "test_routine"), "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    private static IDatabaseDialect CreateFakeDialect() => new FakeDatabaseDialect { ReservedKeywords = ["SELECT"] };
}