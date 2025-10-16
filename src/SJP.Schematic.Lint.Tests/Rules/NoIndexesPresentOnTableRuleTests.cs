using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class NoIndexesPresentOnTableRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoIndexesPresentOnTableRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithoutAnyIndexesOrCandidateKeys_ProducesMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyPrimaryKey_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.Some(Mock.Of<IDatabaseKey>()),
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyUniqueKey_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testUniqueKey = new DatabaseKey(
            Option<Identifier>.Some("test_unique_key"),
            DatabaseKeyType.Unique,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.None,
            [testUniqueKey],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyIndex_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [Mock.Of<IDatabaseIndex>()],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}