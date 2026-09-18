using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal static class LikelyMisspelledNameRuleTests
{
    // Enough distinct words to clear the rule's corpus minimum. Each appears in a single name, so
    // none can be the spelling a suspect word is measured against.
    private static readonly string[] FillerWords =
    [
        "account", "amount", "balance", "branch", "budget", "carrier", "catalog", "channel", "charge", "client",
        "comment", "company", "country", "coupon", "credit", "currency", "delivery", "deposit", "detail", "discount",
        "district", "division", "document", "expiry", "feature", "invoice", "journal", "licence", "listing", "location",
        "manager", "market", "message", "package", "partner", "payment", "period", "picture", "postcode", "premium",
        "product", "profile", "project", "purchase", "receipt", "region", "request", "reserve", "revenue", "sample",
        "schedule", "segment", "service", "session", "setting", "shipment", "status", "summary", "supplier", "voucher",
    ];

    // Shorter than the rule's length floor, so they contribute nothing reportable of their own.
    private static readonly string[] ShortPrefixes =
    [
        "home", "work", "bill", "ship", "post", "main", "alt", "old", "new", "temp",
        "site", "dept", "user", "org", "base",
    ];

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

    private static IReadOnlyCollection<IRelationalDatabaseTable> CreateSchema(params IRelationalDatabaseTable[] tables)
    {
        return
        [
            CreateTable("tbl_filler", FillerWords),
            CreateTable("tbl_support", ShortPrefixes.Select(static p => p + "_address").ToArray()),
            .. tables,
        ];
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(
            () => new LikelyMisspelledNameRule(level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseTables(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInAColumnName_ProducesMessageWithVisibleTableName()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("test_schema", "orders");
        var tables = CreateSchema(CreateTable(suspectTableName, "shipping_adress"));

        var messages = await rule.AnalyseTables(tables);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                messages.Single().Message,
                Is.EqualTo("The column name 'shipping_adress' in the table test_schema.orders contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
            );
            Assert.That(messages.Single().Message, Does.Not.Contain("LocalName ="));
        }
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInTheTableName_ProducesMessageWithVisibleTableName()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("test_schema", "adress_book");
        var tables = CreateSchema(CreateTable(suspectTableName, "id"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo("The name of the table test_schema.adress_book contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenCorrectlySpelledNames_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema(CreateTable("orders", "shipping_address"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
