using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class ForeignKeyMissingRuleTests
{
    private static IRelationalDatabaseTable CreateTable(Identifier tableName, string columnName)
    {
        var column = new DatabaseColumn(columnName, Mock.Of<IDbType>(), true, null, null);
        return new RelationalDatabaseTable(
            tableName,
            [column],
            null,
            [],
            [],
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
        Assert.That(() => new ForeignKeyMissingRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenImpliedRelationshipWithoutForeignKey_ProducesMessageWithVisibleTableNames()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var childTableName = Identifier.CreateQualifiedIdentifier("test_schema", "order_table");
        var targetTableName = Identifier.CreateQualifiedIdentifier("test_schema", "user");
        var childTable = CreateTable(childTableName, "user_id");
        var targetTable = CreateTable(targetTableName, "id");
        var tables = new[] { childTable, targetTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.order_table"));
        Assert.That(message.Message, Does.Contain("test_schema.user"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenNoImpliedRelationship_ProducesNoMessages()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var table = CreateTable("order_table", "quantity");
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
