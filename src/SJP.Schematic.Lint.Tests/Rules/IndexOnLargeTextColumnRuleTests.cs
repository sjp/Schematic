using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class IndexOnLargeTextColumnRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithIndexedColumnType(DataType dataType)
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
            "test",
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
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexOverRegularColumn_ProducesNoMessages()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        var table = CreateTableWithIndexedColumnType(DataType.Integer);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexOverLargeTextColumn_ProducesMessages()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        var table = CreateTableWithIndexedColumnType(DataType.UnicodeText);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexOverLargeBinaryColumn_ProducesMessages()
    {
        var rule = new IndexOnLargeTextColumnRule(RuleLevel.Error);
        var table = CreateTableWithIndexedColumnType(DataType.LargeBinary);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
