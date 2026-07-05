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
internal static class PrimaryKeyColumnNotFirstColumnRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new PrimaryKeyColumnNotFirstColumnRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleColumnNotFirstColumn_ProducesMessageWithVisibleTableName()
    {
        var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn1 = new DatabaseColumn("test_column_1", Mock.Of<IDbType>(), false, null, null);
        var testColumn2 = new DatabaseColumn("test_column_2", Mock.Of<IDbType>(), false, null, null);
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn2],
            true
        );

        var table = new RelationalDatabaseTable(
            tableName,
            [testColumn1, testColumn2],
            testPrimaryKey,
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
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleColumnAsFirstColumn_ProducesNoMessages()
    {
        var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

        var testColumn1 = new DatabaseColumn("test_column_1", Mock.Of<IDbType>(), false, null, null);
        var testColumn2 = new DatabaseColumn("test_column_2", Mock.Of<IDbType>(), false, null, null);
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn1],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn1, testColumn2],
            testPrimaryKey,
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
