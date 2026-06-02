using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ForeignKeySetNullReferentialActionRuleTests
{
    private static DatabaseColumn CreateColumn(string name, bool isNullable)
    {
        return new DatabaseColumn(name, Mock.Of<IDbType>(), isNullable, null, null);
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
        Assert.That(() => new ForeignKeySetNullReferentialActionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithoutSetNullAction_ProducesNoMessages()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", isNullable: false);
        var table = CreateChildTable(column, ReferentialAction.Cascade, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetNullActionWithNullableColumns_ProducesNoMessages()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", isNullable: true);
        var table = CreateChildTable(column, ReferentialAction.SetNull, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetNullDeleteActionWithNonNullableColumns_ProducesMessages()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", isNullable: false);
        var table = CreateChildTable(column, ReferentialAction.SetNull, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetNullUpdateActionWithNonNullableColumns_ProducesMessages()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", isNullable: false);
        var table = CreateChildTable(column, ReferentialAction.NoAction, ReferentialAction.SetNull);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
