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
internal static class OrphanedTableRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new OrphanedTableRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoRelations_ProducesMessageWithVisibleTableName()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var table = new RelationalDatabaseTable(
            tableName,
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
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithParentKeys_ProducesNoMessages()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);

        var parentKey = new DatabaseRelationalKey(
            "child_table",
            new DatabaseKey(
                Option<Identifier>.Some("child_key"),
                DatabaseKeyType.Foreign,
                [Mock.Of<IDatabaseColumn>()],
                true
            ),
            "parent_table",
            new DatabaseKey(
                Option<Identifier>.Some("parent_key"),
                DatabaseKeyType.Primary,
                [Mock.Of<IDatabaseColumn>()],
                true
            ),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [parentKey],
            [],
            [],
            [],
            []
        );
        var tables = new[] { childTable };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
