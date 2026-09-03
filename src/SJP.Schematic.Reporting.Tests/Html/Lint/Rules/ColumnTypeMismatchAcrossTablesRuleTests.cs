using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class ColumnTypeMismatchAcrossTablesRuleTests
{
    // the rule compares what a type describes rather than how it was written, so a definition that
    // stands alone also names the type, e.g. 'integer'
    private static DatabaseColumn CreateColumn(string name, string typeDefinition) => CreateColumn(name, typeDefinition, typeDefinition);

    private static DatabaseColumn CreateColumn(string name, string typeName, string typeDefinition)
    {
        var dbType = new ColumnDataType(
            typeName,
            DataType.Unknown,
            typeDefinition,
            typeof(object),
            false,
            0,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
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
        Assert.That(message.Message, Does.Contain("integer in test_schema.first_table"));
        Assert.That(message.Message, Does.Contain("bigint in test_schema.second_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
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
            Is.EqualTo("The column 'created_at' is declared with differing types across tables: datetime2(7) in alpha, beta; datetime in gamma. Consider using a consistent type to avoid implicit conversions and join errors.")
        );
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
