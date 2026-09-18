using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Rules;

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

    // The base rule renders table names via Identifier.ToString(), whose format is asserted
    // elsewhere. These tests are about detection and ordering, so they reuse it rather than
    // restating it.
    private static string Name(Identifier tableName) => tableName.ToString();

    // Enough tables carrying the same column to clear the support thresholds, so that each test
    // only has to state the case it is actually about.
    private static IEnumerable<IRelationalDatabaseTable> CreateSupportingTables(string columnName, int tableCount)
    {
        return Enumerable
            .Range(1, tableCount)
            .Select(i => CreateTable("supporting_table_" + i.ToString(CultureInfo.InvariantCulture), columnName));
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
    public static async Task AnalyseTables_GivenNoTables_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);

        var messages = await rule.AnalyseTables([]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenColumnNameOneEditFromAWidelyUsedName_ProducesMessage()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 5)
            .Append(CreateTable(Identifier.CreateQualifiedIdentifier("main", "orders"), "custmer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The column 'custmer_id' in the table {Name(Identifier.CreateQualifiedIdentifier("main", "orders"))} is spelled almost identically to 'customer_id', which is used by 5 other tables. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenColumnNameOneEditFromAWidelyUsedName_ReportsAgainstTheTableHoldingIt()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = CreateSupportingTables("email_address", 5)
            .Append(CreateTable(suspectTableName, "email_adress"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().ObjectName.UnwrapSome(), Is.EqualTo(suspectTableName));
    }

    [Test]
    public static async Task AnalyseTables_GivenTransposedCharacters_ProducesMessage()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("quantity", 5)
            .Append(CreateTable("basket", "quantiyt"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().Message, Does.Contain($"The column 'quantiyt' in the table {Name("basket")} is spelled almost identically to 'quantity'"));
    }

    [Test]
    public static async Task AnalyseTables_GivenNamesDifferingOnlyByConvention_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 5)
            .Append(CreateTable("orders", "CustomerId"))
            .Append(CreateTable("baskets", "customerid"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNamesDifferingOnlyByANumericSuffix_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("address1", 5)
            .Append(CreateTable("orders", "address2"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNamesDifferingOnlyByANumberBeingPresent_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("address_line", 5)
            .Append(CreateTable("orders", "address_line1"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenASingularAgainstItsPlural_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("comment_tags", 5)
            .Append(CreateTable("orders", "comment_tag"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenShortNamesOneEditApart_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("idx", 5)
            .Append(CreateTable("orders", "id"))
            .Append(CreateTable("baskets", "dob"))
            .Append(CreateTable("invoices", "doc"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    // A name that is another name with a word boundary bolted on to the front is still only one
    // edit away, and is reported like any other single-character difference. That is the behaviour
    // being pinned rather than a case that is argued for.
    [Test]
    public static async Task AnalyseTables_GivenNameWithASingleLeadingCharacter_ProducesMessage()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("amount", 5)
            .Append(CreateTable("orders", "xamount"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().Message, Does.Contain($"The column 'xamount' in the table {Name("orders")} is spelled almost identically to 'amount'"));
    }

    [Test]
    public static async Task AnalyseTables_GivenAnAbbreviationOfALongerName_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("description", 5)
            .Append(CreateTable("orders", "descr"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSuggestedNameUsedByTooFewTables_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 2)
            .Append(CreateTable("orders", "custmer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenBothNamesUsedByManyTables_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("organisation_name", 5)
            .Concat(Enumerable.Range(1, 5).Select(i => CreateTable("other_table_" + i.ToString(CultureInfo.InvariantCulture), "organization_name")))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSuspectNameUsedByMoreThanOneTable_ProducesNoMessages()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 20)
            .Append(CreateTable("orders", "custmer_id"))
            .Append(CreateTable("baskets", "custmer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSeveralSuspectColumns_OrdersMessagesByTableThenColumn()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 5)
            .Concat(
            [
                CreateTable("zebra", "cusotmer_id"),
                CreateTable("alpha", "custome_id", "custmer_id"),
            ])
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Select(static m => m.Message).ToList(),
            Is.EqualTo(new[]
            {
                $"The column 'custmer_id' in the table {Name("alpha")} is spelled almost identically to 'customer_id', which is used by 5 other tables. Consider whether this is a misspelling.",
                $"The column 'custome_id' in the table {Name("alpha")} is spelled almost identically to 'customer_id', which is used by 5 other tables. Consider whether this is a misspelling.",
                $"The column 'cusotmer_id' in the table {Name("zebra")} is spelled almost identically to 'customer_id', which is used by 5 other tables. Consider whether this is a misspelling.",
            })
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenSuggestedNameAlsoUsedBySuspectTable_CountsOnlyTheOtherTables()
    {
        var rule = new NearDuplicateColumnNameRule(RuleLevel.Error);
        var tables = CreateSupportingTables("customer_id", 5)
            .Append(CreateTable("orders", "customer_id", "custmer_id"))
            .ToList();

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().Message, Does.Contain("which is used by 5 other tables"));
    }
}
