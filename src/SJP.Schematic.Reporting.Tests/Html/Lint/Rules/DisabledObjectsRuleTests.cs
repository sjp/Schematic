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
internal static class DisabledObjectsRuleTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new DisabledObjectsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    private static void AssertVisibleTableNameMessage(IRuleMessage message)
    {
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledPrimaryKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn],
            false
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            testPrimaryKey,
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
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledForeignKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testForeignKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [testColumn],
            false
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn],
            false
        );
        var testRelationalKey = new DatabaseRelationalKey(
            TableName,
            testForeignKey,
            "parent_table",
            testPrimaryKey,
            ReferentialAction.Cascade,
            ReferentialAction.Cascade
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            null,
            [],
            [testRelationalKey],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledUniqueKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testUniqueKey = new DatabaseKey(
            Option<Identifier>.Some("test_unique_key"),
            DatabaseKeyType.Unique,
            [testColumn],
            false
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
            null,
            [testUniqueKey],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledIndex_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var testIndex = new DatabaseIndex(
            "test_index",
            true,
            [new DatabaseIndexColumn("test_column", testColumn, IndexColumnOrder.Ascending)],
            [],
            false,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            TableName,
            [],
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

        Assert.That(messages, Is.Not.Empty);
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledCheck_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testCheck = new DatabaseCheckConstraint(
            Option<Identifier>.Some("test_check"),
            "test_check_definition",
            false
        );

        var table = new RelationalDatabaseTable(
            TableName,
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

        Assert.That(messages, Is.Not.Empty);
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledTrigger_ProducesMessageWithVisibleTableName()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testTrigger = new DatabaseTrigger(
            "test_check",
            "test_check_definition",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            false
        );

        var table = new RelationalDatabaseTable(
            TableName,
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

        Assert.That(messages, Is.Not.Empty);
        AssertVisibleTableNameMessage(messages.Single());
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoDisabledObjects_ProducesNoMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

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
