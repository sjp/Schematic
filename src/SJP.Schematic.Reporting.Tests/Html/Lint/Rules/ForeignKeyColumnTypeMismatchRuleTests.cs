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
internal static class ForeignKeyColumnTypeMismatchRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string typeDefinition)
    {
        var dbType = Mock.Of<IDbType>(t => t.Definition == typeDefinition);
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateChildTable(Identifier childTableName, Identifier parentTableName, IDatabaseColumn childColumn, IDatabaseColumn parentColumn)
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
            [parentColumn],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            childTableName,
            childKey,
            parentTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
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
        Assert.That(() => new ForeignKeyColumnTypeMismatchRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenMismatchingColumnTypes_ProducesMessageWithVisibleTableNames()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childTableName = Identifier.CreateQualifiedIdentifier("test_schema", "child_table");
        var parentTableName = Identifier.CreateQualifiedIdentifier("test_schema", "parent_table");
        var childColumn = CreateColumn("column_2", "text");
        var parentColumn = CreateColumn("column_1", "integer");
        var table = CreateChildTable(childTableName, parentTableName, childColumn, parentColumn);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.child_table"));
        Assert.That(message.Message, Does.Contain("test_schema.parent_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenMatchingColumnTypes_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("column_2", "integer");
        var parentColumn = CreateColumn("column_1", "integer");
        var table = CreateChildTable("child_table", "parent_table", childColumn, parentColumn);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
