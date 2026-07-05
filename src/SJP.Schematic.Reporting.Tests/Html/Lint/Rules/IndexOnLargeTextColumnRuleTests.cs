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
internal static class IndexOnLargeTextColumnRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithIndexedColumnType(Identifier tableName, DataType dataType)
    {
        var dbType = Mock.Of<IDbType>(t => t.DataType == dataType);
        var column = new DatabaseColumn("indexed_column", dbType, true, null, null);
        var indexColumn = new DatabaseIndexColumn("indexed_column", column, IndexColumnOrder.Ascending);
        var index = new DatabaseIndex(
            "test_index",
            false,
            [indexColumn],
            [],
            true,
            Option<string>.None
        );

        return new RelationalDatabaseTable(
            tableName,
            [column],
            null,
            [],
            [],
            [],
            [index],
            [],
            []
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new IndexOnLargeTextColumnRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexOverLargeTextColumn_ProducesMessageWithVisibleTableName()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var table = CreateTableWithIndexedColumnType(tableName, DataType.UnicodeText);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexOverRegularColumn_ProducesNoMessages()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        var table = CreateTableWithIndexedColumnType("test", DataType.Integer);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
