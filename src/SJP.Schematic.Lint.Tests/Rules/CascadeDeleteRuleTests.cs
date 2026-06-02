using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class CascadeDeleteRuleTests
{
    private static IRelationalDatabaseTable CreateChildTable(ReferentialAction deleteAction)
    {
        var column = new DatabaseColumn("test_column", Mock.Of<IDbType>(), false, null, null);
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [column],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [column],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            childKey,
            "parent_table",
            parentKey,
            deleteAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            "child_table",
            [column],
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
        Assert.That(() => new CascadeDeleteRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithoutCascadeDelete_ProducesNoMessages()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        var table = CreateChildTable(ReferentialAction.NoAction);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyWithCascadeDelete_ProducesMessages()
    {
        var rule = new CascadeDeleteRule(RuleLevel.Error);
        var table = CreateChildTable(ReferentialAction.Cascade);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
