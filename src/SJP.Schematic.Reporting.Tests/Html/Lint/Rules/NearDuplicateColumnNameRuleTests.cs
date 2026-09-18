using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal static class NearDuplicateColumnNameRuleTests
{
    private static DatabaseColumn CreateColumn(string name)
    {
        var dbType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(int),
            false,
            0,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName, params string[] columnNames)
    {
        return new RelationalDatabaseTable(
            tableName,
            columnNames.Select(CreateColumn).ToList(),
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
        Assert.That(
            () => new NearDuplicateColumnNameRule(level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseTables(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenNearDuplicateColumnName_ProducesMessageWithVisibleTableName()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("test_schema", "orders");
        var tables = Enumerable
            .Range(1, 5)
            .Select(i => CreateTable(Identifier.CreateQualifiedIdentifier("test_schema", "supporting_table_" + i.ToString(CultureInfo.InvariantCulture)), "customer_id"))
            .Append(CreateTable(suspectTableName, "custmer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                messages.Single().Message,
                Is.EqualTo("The column 'custmer_id' in the table test_schema.orders is spelled almost identically to 'customer_id', which is used by 5 other tables. Consider whether this is a misspelling.")
            );
            Assert.That(messages.Single().Message, Does.Not.Contain("LocalName ="));
        }
    }

    [Test]
    public static async Task AnalyseTables_GivenConsistentColumnNames_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = Enumerable
            .Range(1, 5)
            .Select(i => CreateTable("table_" + i.ToString(CultureInfo.InvariantCulture), "customer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
