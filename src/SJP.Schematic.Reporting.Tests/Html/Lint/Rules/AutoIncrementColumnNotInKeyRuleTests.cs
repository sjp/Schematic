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
internal static class AutoIncrementColumnNotInKeyRuleTests
{
    private static DatabaseColumn CreateAutoIncrementColumn(string name)
    {
        return new DatabaseColumn(
            name,
            Mock.Of<IDbType>(),
            false,
            null,
            Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new AutoIncrementColumnNotInKeyRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenAutoIncrementColumnNotInAnyKey_ProducesMessageWithVisibleTableName()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = CreateAutoIncrementColumn("counter");
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var table = new RelationalDatabaseTable(
            tableName,
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
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenAutoIncrementColumnInPrimaryKey_ProducesNoMessages()
    {
        var rule = new AutoIncrementColumnNotInKeyRule(RuleLevel.Error);
        var column = CreateAutoIncrementColumn("id");
        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("pk_test"),
            DatabaseKeyType.Primary,
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
}
