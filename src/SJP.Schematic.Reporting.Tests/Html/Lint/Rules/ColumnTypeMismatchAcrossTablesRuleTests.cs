using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class ColumnTypeMismatchAcrossTablesRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string typeDefinition)
    {
        var dbType = Mock.Of<IDbType>(t => t.Definition == typeDefinition);
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName, IDatabaseColumn column)
    {
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
        Assert.That(() => new ColumnTypeMismatchAcrossTablesRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenSameNamedColumnWithDifferentTypes_ProducesMessageWithVisibleTableNames()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var firstTableName = Identifier.CreateQualifiedIdentifier("test_schema", "first_table");
        var secondTableName = Identifier.CreateQualifiedIdentifier("test_schema", "second_table");
        var firstTable = CreateTable(firstTableName, CreateColumn("user_id", "integer"));
        var secondTable = CreateTable(secondTableName, CreateColumn("user_id", "bigint"));
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.first_table"));
        Assert.That(message.Message, Does.Contain("test_schema.second_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenConsistentColumnTypes_ProducesNoMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var firstTable = CreateTable("first", CreateColumn("user_id", "integer"));
        var secondTable = CreateTable("second", CreateColumn("user_id", "integer"));
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
