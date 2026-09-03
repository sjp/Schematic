using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ColumnTypeMismatchAcrossTablesRuleTests
{
    // the rule compares what a type describes rather than how it was written, so a definition that
    // stands alone also names the type, e.g. 'integer'
    private static DatabaseColumn CreateColumn(string name, string typeDefinition) => CreateColumn(name, typeDefinition, typeDefinition);

    private static DatabaseColumn CreateColumn(string name, string typeName, string typeDefinition, int maxLength = 0)
    {
        var dbType = new ColumnDataType(
            typeName,
            DataType.Unknown,
            typeDefinition,
            typeof(object),
            false,
            maxLength,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
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
            CreateTable("beta", CreateColumn("created_at", "datetime2", "datetime2(7)")),
            CreateTable("alpha", CreateColumn("created_at", "datetime2", "datetime2(7)")),
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
        var alpha = CreateTable("alpha", CreateColumn("created_at", "datetime2", "datetime2(7)"));
        var beta = CreateTable("beta", CreateColumn("created_at", "datetime2", "datetime2(7)"));
        var gamma = CreateTable("gamma", CreateColumn("created_at", "datetime"));

        var messages = await rule.AnalyseTables([alpha, beta, gamma]);
        var reorderedMessages = await rule.AnalyseTables([gamma, beta, alpha]);

        Assert.That(reorderedMessages.Single().Message, Is.EqualTo(messages.Single().Message));
    }

    [Test]
    public static async Task AnalyseTables_GivenSameTypeWrittenDifferently_ProducesNoMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateTable("first", CreateColumn("name", "varchar", "varchar(50)", 50)),
            CreateTable("second", CreateColumn("name", "VARCHAR", "VARCHAR(50)", 50)),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSameTypeNameWithDifferentLengths_ProducesMessages()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateTable("first", CreateColumn("name", "varchar", "varchar(50)", 50)),
            CreateTable("second", CreateColumn("name", "varchar", "varchar(100)", 100)),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenGroupWithDifferentlyWrittenDefinitions_NamesGroupIdenticallyWhateverTheTableOrder()
    {
        var rule = new ColumnTypeMismatchAcrossTablesRule(RuleLevel.Error);
        var upper = CreateTable("first", CreateColumn("name", "varchar", "VARCHAR(50)", 50));
        var lower = CreateTable("second", CreateColumn("name", "varchar", "varchar(50)", 50));
        var other = CreateTable("third", CreateColumn("name", "text", "text"));

        var messages = await rule.AnalyseTables([upper, lower, other]);
        var reorderedMessages = await rule.AnalyseTables([lower, upper, other]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(messages.Single().Message, Does.Contain($"VARCHAR(50) in {Name("first")}, {Name("second")}"));
            Assert.That(reorderedMessages.Single().Message, Is.EqualTo(messages.Single().Message));
        }
    }
}
