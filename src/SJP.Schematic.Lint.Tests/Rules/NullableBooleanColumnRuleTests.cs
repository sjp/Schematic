using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class NullableBooleanColumnRuleTests
{
    private static DatabaseColumn CreateColumn(string name, DataType dataType, bool isNullable)
    {
        var dbType = Mock.Of<IDbType>(t => t.DataType == dataType);
        return new DatabaseColumn(name, dbType, isNullable, null, null);
    }

    private static IRelationalDatabaseTable CreateTable(IDatabaseColumn column)
    {
        return new RelationalDatabaseTable(
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
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenNonNullableBooleanColumn_ProducesNoMessages()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        var table = CreateTable(CreateColumn("is_active", DataType.Boolean, isNullable: false));
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNullableNonBooleanColumn_ProducesNoMessages()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        var table = CreateTable(CreateColumn("description", DataType.String, isNullable: true));
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNullableBooleanColumn_ProducesMessages()
    {
        var rule = new NullableBooleanColumnRule(RuleLevel.Error);
        var table = CreateTable(CreateColumn("is_active", DataType.Boolean, isNullable: true));
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}
