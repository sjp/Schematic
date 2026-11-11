using System;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class NoIndexesPresentOnTableRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(
            () => new NoIndexesPresentOnTableRule(level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseTables(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [TestCase(TableKind.History)]
    [TestCase(TableKind.Partition)]
    [TestCase(TableKind.PartitionParent)]
    public static async Task AnalyseTables_GivenDatabaseManagedTableWithoutAnyIndexesOrCandidateKeys_ProducesNoMessages(TableKind kind)
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
        var tables = new[] { CreateTable(kind) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    private static RelationalDatabaseTable CreateTable(TableKind kind)
    {
        return new RelationalDatabaseTable(
            "test",
            [],
            Option<DatabaseKey.Primary>.None,
            [],
            [],
            [],
            [],
            [],
            [],
            kind,
            Option<ITablePartitioning>.None,
            Option<ITableSystemVersioning>.None,
            true,
            Option<Identifier>.None
        );
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
            Option<DatabaseKey.Primary>.Some(TestKeys.Primary()),
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
            Option<DatabaseDefaultValue>.None,
            new ColumnValueSource.Stored()
        );
        var testUniqueKey = new DatabaseKey.Unique(
            Option<Identifier>.Some("test_unique_key"),
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<DatabaseKey.Primary>.None,
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
            Option<DatabaseKey.Primary>.None,
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