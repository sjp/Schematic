using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

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
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOneColumn_ProducesMessageWithVisibleTableName()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), true, null, null);

        var table = new RelationalDatabaseTable(
            tableName,
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

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoColumns_ProducesMessageMentioningNoColumns()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);
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
        Assert.That(message.Message, Does.Contain("no columns"));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnsExceedingLimit_ProducesNoMessages()
    {
        var rule = new OnlyOneColumnPresentRule(RuleLevel.Error);

        var testColumn1 = new DatabaseColumn("test_column_1", Mock.Of<IDbType>(), true, null, null);
        var testColumn2 = new DatabaseColumn("test_column_2", Mock.Of<IDbType>(), true, null, null);

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn1, testColumn2],
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
