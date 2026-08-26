using System.Linq;
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

    // The base rule renders table names via Identifier.ToString(), whose format is asserted elsewhere.
    // These tests are about grouping and ordering, so they reuse it rather than restating it.
    private static string Name(string tableName) => new Identifier(tableName).ToString();

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

    [Test]
    public static async Task AnalyseTables_GivenSameNamedColumnWithDifferentTypes_NamesEachTypeAndItsTables()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateTable("beta", CreateColumn("created_at", "datetime2(7)")),
            CreateTable("alpha", CreateColumn("created_at", "datetime2(7)")),
            CreateTable("gamma", CreateColumn("created_at", "datetime")),
        };

        var messages = await rule.AnalyseTables(tables);

        var message = messages.Single();
        Assert.That(
            message.Message,
            Is.EqualTo($"The column 'created_at' is declared with differing types across tables: datetime2(7) in {Name("alpha")}, {Name("beta")}; datetime in {Name("gamma")}. Consider using a consistent type to avoid implicit conversions and join errors.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenTypeGroupsOfEqualSize_OrdersGroupsByTypeDefinition()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateTable("second", CreateColumn("user_id", "integer")),
            CreateTable("first", CreateColumn("user_id", "bigint")),
        };

        var messages = await rule.AnalyseTables(tables);

        var message = messages.Single();
        Assert.That(
            message.Message,
            Is.EqualTo($"The column 'user_id' is declared with differing types across tables: bigint in {Name("first")}; integer in {Name("second")}. Consider using a consistent type to avoid implicit conversions and join errors.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenTablesInDifferentOrder_ProducesIdenticalMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var alpha = CreateTable("alpha", CreateColumn("created_at", "datetime2(7)"));
        var beta = CreateTable("beta", CreateColumn("created_at", "datetime2(7)"));
        var gamma = CreateTable("gamma", CreateColumn("created_at", "datetime"));

        var messages = await rule.AnalyseTables([alpha, beta, gamma]);
        var reorderedMessages = await rule.AnalyseTables([gamma, beta, alpha]);

        Assert.That(reorderedMessages.Single().Message, Is.EqualTo(messages.Single().Message));
    }
}
