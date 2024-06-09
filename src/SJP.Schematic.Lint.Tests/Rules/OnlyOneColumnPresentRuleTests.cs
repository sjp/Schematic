using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class OnlyOneColumnPresentRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new OnlyOneColumnPresentRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnsExceedingLimit_ProducesMessages()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);

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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOneColumn_ProducesMessages()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);

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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}