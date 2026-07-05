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
internal static class PrimaryKeyNotIntegerRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new PrimaryKeyNotIntegerRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleNonIntegerColumn_ProducesMessageWithVisibleTableName()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var dataTypeMock = new Mock<IDbType>(MockBehavior.Strict);
        dataTypeMock.Setup(t => t.DataType).Returns(DataType.Binary);

        var testColumn = new DatabaseColumn("test_column_1", dataTypeMock.Object, false, null, null);
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            tableName,
            [testColumn],
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
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleIntegerColumn_ProducesNoMessages()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

        var dataTypeMock = new Mock<IDbType>(MockBehavior.Strict);
        dataTypeMock.Setup(t => t.DataType).Returns(DataType.Integer);

        var testColumn = new DatabaseColumn("test_column_1", dataTypeMock.Object, false, null, null);
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn],
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
