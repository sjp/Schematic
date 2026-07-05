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
internal static class CascadeDeleteRuleTests
{
    private static IRelationalDatabaseTable CreateChildTable(ReferentialAction deleteAction, Identifier childTableName, Identifier parentTableName)
    {
        var column = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [column],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [column],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            childTableName,
            childKey,
            parentTableName,
            parentKey,
            deleteAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            childTableName,
            [column],
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
        Assert.That(() => new CascadeDeleteRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithCascadeDelete_ProducesMessageWithVisibleTableNames()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        var childTableName = Identifier.CreateQualifiedIdentifier("test_schema", "child_table");
        var parentTableName = Identifier.CreateQualifiedIdentifier("test_schema", "parent_table");
        var table = CreateChildTable(ReferentialAction.Cascade, childTableName, parentTableName);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.child_table"));
        Assert.That(message.Message, Does.Contain("test_schema.parent_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithoutCascadeDelete_ProducesNoMessages()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        var table = CreateChildTable(ReferentialAction.NoAction, "child_table", "parent_table");
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
