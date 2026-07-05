using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class TooManyColumnsRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new TooManyColumnsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenZeroColumnLimit_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => new TooManyColumnsRule(RuleLevel.Error, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithColumnsExceedingLimit_ProducesMessageWithVisibleTableName()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error, 2);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var testColumn1 = new DatabaseColumn("test_column_1", Mock.Of<IDbType>(), true, null, null);
        var testColumn2 = new DatabaseColumn("test_column_2", Mock.Of<IDbType>(), true, null, null);
        var testColumn3 = new DatabaseColumn("test_column_3", Mock.Of<IDbType>(), true, null, null);

        var table = new RelationalDatabaseTable(
            tableName,
            [testColumn1, testColumn2, testColumn3],
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
    public static async Task AnalyseTables_GivenTableWithLimitedNumberOfColumns_ProducesNoMessages()
    {
        var rule = new TooManyColumnsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn("test_column", Mock.Of<IDbType>(), true, null, null);

        var table = new RelationalDatabaseTable(
            "test",
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

        Assert.That(messages, Is.Empty);
    }
}
