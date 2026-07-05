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
internal static class ForeignKeySetNullReferentialActionRuleTests
{
    private static DatabaseColumn CreateColumn(string name, bool isNullable)
    {
        return new DatabaseColumn(name, Mock.Of<IDbType>(), isNullable, null, null);
    }

    private static IRelationalDatabaseTable CreateChildTable(Identifier childTableName, IDatabaseColumn childColumn, ReferentialAction deleteAction, ReferentialAction updateAction)
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
            childTableName,
            childKey,
            "parent_table",
            parentKey,
            deleteAction,
            updateAction
        );

        return new RelationalDatabaseTable(
            childTableName,
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
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetNullDeleteActionWithNonNullableColumns_ProducesMessageWithVisibleTableName()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var childTableName = Identifier.CreateQualifiedIdentifier("test_schema", "child_table");
        var column = CreateColumn("test_column", false);
        var table = CreateChildTable(childTableName, column, ReferentialAction.SetNull, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.child_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenSetNullActionWithNullableColumns_ProducesNoMessages()
    {
        var rule = new ForeignKeySetNullReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", true);
        var table = CreateChildTable("child_table", column, ReferentialAction.SetNull, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
