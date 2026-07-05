using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class InconsistentColumnNamingConventionRuleTests
{
    private static IRelationalDatabaseTable CreateTable(Identifier tableName, params string[] columnNames)
    {
        var columns = columnNames
            .Select(static name => (IDatabaseColumn)new DatabaseColumn(name, Mock.Of<IDbType>(), true, null, null))
            .ToList();

        return new RelationalDatabaseTable(
            tableName,
            columns,
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
        Assert.That(() => new InconsistentColumnNamingConventionRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new InconsistentColumnNamingConventionRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenMixedNamingConventions_ProducesMessageWithVisibleTableName()
    {
        var rule = new InconsistentColumnNamingConventionRule(RuleLevel.Error);
        var firstTable = CreateTable(Identifier.CreateQualifiedIdentifier("test_schema", "first"), "user_id", "first_name");
        var secondTable = CreateTable(Identifier.CreateQualifiedIdentifier("test_schema", "test_table"), "OrderId");
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single(m => m.Message.Contains("OrderId"));
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenConsistentSnakeCaseColumns_ProducesNoMessages()
    {
        var rule = new InconsistentColumnNamingConventionRule(RuleLevel.Error);
        var firstTable = CreateTable("first", "user_id", "first_name");
        var secondTable = CreateTable("second", "order_id", "created_at");
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
