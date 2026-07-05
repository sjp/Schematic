using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

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
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseSequences(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseSynonyms(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseRoutines(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNameContainingWhitespace_ProducesMessageWithVisibleTableName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "   test   ");

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
        Assert.That(message.Message, Does.Contain("test_schema."));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnNameContainingWhitespace_ProducesMessageWithVisibleTableName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("   test_column ", Mock.Of<IDbType>(), false, null, null);

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
    public static async Task AnalyseViews_GivenViewWithNameContainingWhitespace_ProducesMessageWithVisibleViewName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "   test   ");

        var view = new DatabaseView(viewName, "select 1", []);
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema."));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseViews_GivenViewWithColumnNameContainingWhitespace_ProducesMessageWithVisibleViewName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "test_view");

        var testColumn = new DatabaseColumn("   test_column   ", Mock.Of<IDbType>(), false, null, null);
        var view = new DatabaseView(viewName, "select 1", [testColumn]);
        var views = new[] { view };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_view"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseSequences_GivenSequenceWithNameContainingWhitespace_ProducesMessageWithVisibleSequenceName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "   test   ");

        var sequence = new DatabaseSequence(sequenceName, 1, 1, 1, 100, true, 10);
        var sequences = new[] { sequence };

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema."));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenSynonymWithNameContainingWhitespace_ProducesMessageWithVisibleSynonymName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var synonymName = Identifier.CreateQualifiedIdentifier("test_schema", "   test   ");

        var synonym = new DatabaseSynonym(synonymName, "target");
        var synonyms = new[] { synonym };

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema."));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseRoutines_GivenRoutineWithNameContainingWhitespace_ProducesMessageWithVisibleRoutineName()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);
        var routineName = Identifier.CreateQualifiedIdentifier("test_schema", "   test   ");

        var routine = new DatabaseRoutine(routineName, "routine_definition");
        var routines = new[] { routine };

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema."));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRegularName_ProducesNoMessages()
    {
        var rule = new WhitespaceNameRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
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
}
