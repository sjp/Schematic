using System;
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
internal static class TooManyIndexColumnsRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithIndexColumnCount(Identifier tableName, int columnCount)
    {
        var indexColumns = Enumerable.Range(1, columnCount)
            .Select(i =>
            {
                var column = new DatabaseColumn("col_" + i.ToString(), Mock.Of<IDbType>(), true, null, null);
                return (IDatabaseIndexColumn)new DatabaseIndexColumn("col_" + i.ToString(), column, IndexColumnOrder.Ascending);
            })
            .ToList();

        var index = new DatabaseIndex(
            "test_index",
            false,
            indexColumns,
            [],
            true,
            Option<string>.None
        );

        return new RelationalDatabaseTable(
            tableName,
            [],
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
        Assert.That(() => new TooManyIndexColumnsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenZeroColumnLimit_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => new TooManyIndexColumnsRule(RuleLevel.Error, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new TooManyIndexColumnsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexExceedingColumnLimit_ProducesMessageWithVisibleTableName()
    {
        var rule = new TooManyIndexColumnsRule(RuleLevel.Error, 2);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var table = CreateTableWithIndexColumnCount(tableName, 3);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexWithinColumnLimit_ProducesNoMessages()
    {
        var rule = new TooManyIndexColumnsRule(RuleLevel.Error, 3);
        var table = CreateTableWithIndexColumnCount("test", 2);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
