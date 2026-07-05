using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class NullableBooleanColumnRuleTests
{
    private static DatabaseColumn CreateColumn(string name, DataType dataType, bool isNullable)
    {
        var dbType = Mock.Of<IDbType>(t => t.DataType == dataType);
        return new DatabaseColumn(name, dbType, isNullable, null, null);
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName, IDatabaseColumn column)
    {
        return new RelationalDatabaseTable(
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
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NullableBooleanColumnRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenNullableBooleanColumn_ProducesMessageWithVisibleTableName()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
        var table = CreateTable(tableName, CreateColumn("is_active", DataType.Boolean, isNullable: true));
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenNonNullableBooleanColumn_ProducesNoMessages()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        var table = CreateTable("test", CreateColumn("is_active", DataType.Boolean, isNullable: false));
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
