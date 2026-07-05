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
internal static class ForeignKeyRelationshipCycleRuleTests
{
    private static IRelationalDatabaseTable CreateTable(Identifier tableName, Identifier referencedTableName)
    {
        var column = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(Option<Identifier>.Some("test_fk"), DatabaseKeyType.Foreign, [column], true);
        var parentKey = new DatabaseKey(Option<Identifier>.Some("test_pk"), DatabaseKeyType.Primary, [column], true);
        var relationalKey = new DatabaseRelationalKey(
            tableName,
            childKey,
            referencedTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            tableName,
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
        Assert.That(() => new ForeignKeyRelationshipCycleRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTablesFormingACycle_ProducesMessageWithVisibleTableNames()
    {
        var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
        var tableAName = Identifier.CreateQualifiedIdentifier("test_schema", "table_a");
        var tableBName = Identifier.CreateQualifiedIdentifier("test_schema", "table_b");
        var tableA = CreateTable(tableAName, tableBName);
        var tableB = CreateTable(tableBName, tableAName);
        var tables = new[] { tableA, tableB };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.table_a"));
        Assert.That(message.Message, Does.Contain("test_schema.table_b"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTablesWithoutACycle_ProducesNoMessages()
    {
        var rule = new ForeignKeyRelationshipCycleRule(RuleLevel.Error);
        var parentColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var parentTable = new RelationalDatabaseTable(
            "parent_table",
            [parentColumn],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var childTable = CreateTable("child_table", "parent_table");
        var tables = new[] { parentTable, childTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
