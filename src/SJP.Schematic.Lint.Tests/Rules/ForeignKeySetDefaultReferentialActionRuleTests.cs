using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ForeignKeySetDefaultReferentialActionRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string defaultValue)
    {
        return new DatabaseColumn(name, Mock.Of<IDbType>(), true, defaultValue, null);
    }

    private static IRelationalDatabaseTable CreateChildTable(IDatabaseColumn childColumn, ReferentialAction deleteAction, ReferentialAction updateAction)
    {
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [childColumn],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [childColumn],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            childKey,
            "parent_table",
            parentKey,
            deleteAction,
            updateAction
        );

        return new RelationalDatabaseTable(
            "child_table",
            [childColumn],
            null,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeySetDefaultReferentialActionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithoutSetDefaultAction_ProducesNoMessages()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", defaultValue: null);
        var table = CreateChildTable(column, ReferentialAction.Cascade, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetDefaultActionWithDefaultValue_ProducesNoMessages()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", defaultValue: "0");
        var table = CreateChildTable(column, ReferentialAction.SetDefault, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetDefaultDeleteActionWithoutDefaultValue_ProducesMessages()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", defaultValue: null);
        var table = CreateChildTable(column, ReferentialAction.SetDefault, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetDefaultUpdateActionWithoutDefaultValue_ProducesMessages()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", defaultValue: null);
        var table = CreateChildTable(column, ReferentialAction.NoAction, ReferentialAction.SetDefault);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
