using System;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class AutoIncrementColumnNotInKeyRuleTests
{
    private static DatabaseColumn CreateAutoIncrementColumn(string name)
    {
        return new DatabaseColumn(
            name,
            Mock.Of<IDbType>(),
            false,
            Option<DatabaseDefaultValue>.None,
            new ColumnValueSource.Generated(new AutoIncrement(1, 1))
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(
            () => new AutoIncrementColumnNotInKeyRule(level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseTables(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenAutoIncrementColumnInPrimaryKey_ProducesNoMessages()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = CreateAutoIncrementColumn("id");
        var primaryKey = new DatabaseKey.Primary(
            Option<Identifier>.Some("pk_test"),
            [column],
            true
        );
        var table = new RelationalDatabaseTable(
            "test",
            [column],
            primaryKey,
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
    public static async Task AnalyseTables_GivenAutoIncrementColumnInUniqueKey_ProducesNoMessages()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = CreateAutoIncrementColumn("id");
        var uniqueKey = new DatabaseKey.Unique(
            Option<Identifier>.Some("uk_test"),
            [column],
            true
        );
        var table = new RelationalDatabaseTable(
            "test",
            [column],
            null,
            [uniqueKey],
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
    public static async Task AnalyseTables_GivenAutoIncrementColumnNotInAnyKey_ProducesMessages()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = CreateAutoIncrementColumn("counter");
        var table = new RelationalDatabaseTable(
            "test",
            [column],
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

    private static readonly ColumnValueSource[] ValueSourcesWithoutIdentity =
    [
        new ColumnValueSource.Stored(),
        new ColumnValueSource.Computed(Option<string>.Some("1 + 1"), ComputedColumnStorage.Stored),
        new ColumnValueSource.Computed(Option<string>.None, ComputedColumnStorage.Virtual),
    ];

    [TestCaseSource(nameof(ValueSourcesWithoutIdentity))]
    public static async Task AnalyseTables_GivenColumnWithoutIdentityNotInAnyKey_ProducesNoMessages(ColumnValueSource valueSource)
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = new DatabaseColumn("counter", Mock.Of<IDbType>(), false, Option<DatabaseDefaultValue>.None, valueSource);
        var table = new RelationalDatabaseTable(
            "test",
            [column],
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

        Assert.That(messages, Is.Empty);
    }
}
