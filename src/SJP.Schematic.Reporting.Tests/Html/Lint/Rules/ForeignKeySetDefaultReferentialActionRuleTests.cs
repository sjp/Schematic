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
internal static class ForeignKeySetDefaultReferentialActionRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string defaultValue)
    {
        return new DatabaseColumn(name, Mock.Of<IDbType>(), true, defaultValue, null);
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
        Assert.That(() => new ForeignKeySetDefaultReferentialActionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenSetDefaultDeleteActionWithoutDefaultValue_ProducesMessageWithVisibleTableName()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var childTableName = Identifier.CreateQualifiedIdentifier("test_schema", "child_table");
        var column = CreateColumn("test_column", null);
        var table = CreateChildTable(childTableName, column, ReferentialAction.SetDefault, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.child_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenSetDefaultActionWithDefaultValue_ProducesNoMessages()
    {
        var rule = new ForeignKeySetDefaultReferentialActionRule(RuleLevel.Error);
        var column = CreateColumn("test_column", "0");
        var table = CreateChildTable("child_table", column, ReferentialAction.SetDefault, ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
