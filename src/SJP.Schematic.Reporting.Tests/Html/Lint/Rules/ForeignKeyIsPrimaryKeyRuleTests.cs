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
internal static class ForeignKeyIsPrimaryKeyRuleTests
{
    private static IRelationalDatabaseTable CreateTable(Identifier tableName, Identifier otherTableName)
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
            tableName,
            childKey,
            otherTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            tableName,
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
        Assert.That(() => new ForeignKeyIsPrimaryKeyRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyMatchingPrimaryKeyOnSameTable_ProducesMessageWithVisibleTableName()
    {
        var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var table = CreateTable(tableName, tableName);
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenForeignKeyReferencingDifferentTable_ProducesNoMessages()
    {
        var rule = new ForeignKeyIsPrimaryKeyRule(RuleLevel.Error);
        var table = CreateTable("child_table", "parent_table");
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
