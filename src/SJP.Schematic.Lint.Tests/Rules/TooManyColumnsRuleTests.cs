using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class TooManyColumnsRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new TooManyColumnsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenZeroColumnLimit_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => new TooManyColumnsRule(RuleLevel.Error, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithLimitedNumberOfColumns_ProducesNoMessages()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            true,
            null,
            null
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnsExceedingLimit_ProducesMessages()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error, 2);

        var testColumn1 = new DatabaseColumn(
            "test_column_1",
            Mock.Of<IDbType>(),
            true,
            null,
            null
        );

        var testColumn2 = new DatabaseColumn(
            "test_column_2",
            Mock.Of<IDbType>(),
            true,
            null,
            null
        );

        var testColumn3 = new DatabaseColumn(
            "test_column_3",
            Mock.Of<IDbType>(),
            true,
            null,
            null
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn1, testColumn2, testColumn3],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Not.Empty);
    }
}