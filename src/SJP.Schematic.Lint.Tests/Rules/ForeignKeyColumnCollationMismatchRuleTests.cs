using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ForeignKeyColumnCollationMismatchRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string collation)
    {
        var collationOption = collation is null ? Option<Identifier>.None : Option<Identifier>.Some(collation);
        var dbType = Mock.Of<IDbType>(t => t.Collation == collationOption);
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateChildTable(IDatabaseColumn childColumn, IDatabaseColumn parentColumn)
    {
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [childColumn],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [parentColumn],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            childKey,
            "parent_table",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            "child_table",
            [childColumn],
            null,
            [],
            [relationalKey],
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
        Assert.That(() => new ForeignKeyColumnCollationMismatchRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyColumnCollationMismatchRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenMatchingCollations_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnCollationMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("name", "Latin1_General_CI_AS");
        var parentColumn = CreateColumn("name", "Latin1_General_CI_AS");
        var table = CreateChildTable(childColumn, parentColumn);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNoCollationsOnEitherColumn_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnCollationMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("id", null);
        var parentColumn = CreateColumn("id", null);
        var table = CreateChildTable(childColumn, parentColumn);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenMismatchingCollations_ProducesMessages()
    {
        var rule = new ForeignKeyColumnCollationMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("name", "Latin1_General_CI_AS");
        var parentColumn = CreateColumn("name", "SQL_Latin1_General_CP1_CS_AS");
        var table = CreateChildTable(childColumn, parentColumn);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
