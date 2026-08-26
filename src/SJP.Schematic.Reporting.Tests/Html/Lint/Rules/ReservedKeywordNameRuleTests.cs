using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class ReservedKeywordNameRuleTests
{
    [Test]
    public static async Task AnalyseTables_GivenTableWithIndexNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

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
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("SELECT"), DatabaseKeyType.Primary, [testColumn], true);

        var table = new RelationalDatabaseTable(
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUniqueKeyNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("SELECT"), DatabaseKeyType.Unique, [testColumn], true);

        var table = new RelationalDatabaseTable(
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithForeignKeyNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(Option<Identifier>.Some("SELECT"), DatabaseKeyType.Foreign, [testColumn], true);
        var parentKey = new DatabaseKey(Option<Identifier>.Some("parent_pk"), DatabaseKeyType.Primary, [testColumn], true);
        var relationalKey = new DatabaseRelationalKey(
            tableName,
            childKey,
            "parent_table",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var table = new RelationalDatabaseTable(
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithCheckConstraintNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testCheck = new DatabaseCheckConstraint(Option<Identifier>.Some("SELECT"), "test_check_definition", true);

        var table = new RelationalDatabaseTable(
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithTriggerNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testTrigger = new DatabaseTrigger(
            "SELECT",
            "test_trigger_definition",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            true
        );

        var table = new RelationalDatabaseTable(
            tableName,
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

        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    private static IDatabaseDialect CreateFakeDialect() => Mock.Of<IDatabaseDialect>(d => d.IsReservedKeyword("SELECT"));

    [Test]
    public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
    {
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new ReservedKeywordNameRule(null!, level), Throws.ArgumentNullException);
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
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseSequences(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseSynonyms(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        Assert.That(() => rule.AnalyseRoutines(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "SELECT");

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
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.SELECT"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessageWithVisibleTableName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("SELECT", Mock.Of<IDbType>(), false, null, null);

        var table = new RelationalDatabaseTable(
            tableName,
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
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithNameContainingReservedKeyword_ProducesMessageWithVisibleViewName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "SELECT");

        var view = new DatabaseView(viewName, "select 1", []);
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.SELECT"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessageWithVisibleViewName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "test_view");

        var testColumn = new DatabaseColumn("SELECT", Mock.Of<IDbType>(), false, null, null);
        var view = new DatabaseView(viewName, "select 1", [testColumn]);
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_view"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithNameContainingReservedKeyword_ProducesMessageWithVisibleSequenceName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "SELECT");

        var sequence = new DatabaseSequence(sequenceName, 1, 1, 1, 100, true, 10);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.SELECT"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingReservedKeyword_ProducesMessageWithVisibleSynonymName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var synonymName = Identifier.CreateQualifiedIdentifier("test_schema", "SELECT");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.SELECT"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithNameContainingReservedKeyword_ProducesMessageWithVisibleRoutineName()
    {
        var rule = new ReservedKeywordNameRule(CreateFakeDialect(), RuleLevel.Error);
        var routineName = Identifier.CreateQualifiedIdentifier("test_schema", "SELECT");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.SELECT"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }
}
