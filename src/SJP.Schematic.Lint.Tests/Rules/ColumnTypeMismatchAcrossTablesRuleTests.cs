using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ColumnTypeMismatchAcrossTablesRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string typeDefinition)
    {
        var dbType = Mock.Of<IDbType>(t => t.Definition == typeDefinition);
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateTable(string tableName, IDatabaseColumn column)
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
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
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

    [Test]
    public static async Task AnalyseTables_GivenDifferentlyNamedColumns_ProducesNoMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var firstTable = CreateTable("first", CreateColumn("user_id", "integer"));
        var secondTable = CreateTable("second", CreateColumn("order_id", "bigint"));
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSameNamedColumnWithDifferentTypes_ProducesMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var firstTable = CreateTable("first", CreateColumn("user_id", "integer"));
        var secondTable = CreateTable("second", CreateColumn("user_id", "bigint"));
        var tables = new[] { firstTable, secondTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
