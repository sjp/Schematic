using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class TooManyIndexColumnsRuleTests
{
    private static IRelationalDatabaseTable CreateTableWithIndexColumnCount(int columnCount)
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
            "test",
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
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexWithinColumnLimit_ProducesNoMessages()
    {
        var rule = new TooManyIndexColumnsRule(RuleLevel.Error, 3);
        var table = CreateTableWithIndexColumnCount(2);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenIndexExceedingColumnLimit_ProducesMessages()
    {
        var rule = new TooManyIndexColumnsRule(RuleLevel.Error, 2);
        var table = CreateTableWithIndexColumnCount(3);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
