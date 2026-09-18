using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class LikelyMisspelledNameRuleTests
{
    // Enough distinct words to clear the rule's corpus minimum, so that each test only has to
    // state the case it is actually about. Every one of them appears in a single name, so none can
    // ever be the spelling a suspect word is measured against.
    private static readonly string[] FillerWords =
    [
        "account", "amount", "balance", "branch", "budget", "carrier", "catalog", "channel", "charge", "client",
        "comment", "company", "country", "coupon", "credit", "currency", "delivery", "deposit", "detail", "discount",
        "district", "division", "document", "expiry", "feature", "invoice", "journal", "licence", "listing", "location",
        "manager", "market", "message", "package", "partner", "payment", "period", "picture", "postcode", "premium",
        "product", "profile", "project", "purchase", "receipt", "region", "request", "reserve", "revenue", "sample",
        "schedule", "segment", "service", "session", "setting", "shipment", "status", "summary", "supplier", "voucher",
    ];

    // Prefixes used to spell one word across many distinct names. All are shorter than the rule's
    // length floor, so they contribute nothing that could be reported in its own right.
    private static readonly string[] ShortPrefixes =
    [
        "home", "work", "bill", "ship", "post", "main", "alt", "old", "new", "temp",
        "site", "dept", "user", "org", "base", "free", "held", "late", "lead", "void",
    ];

    // The base rule renders object names via Identifier.ToString(), whose format is asserted
    // elsewhere. These tests are about detection and ordering, so they reuse it rather than
    // restating it.
    private static string Name(Identifier objectName) => objectName.ToString();

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
        return CreateTable(tableName, columnNames, [], []);
    }

    private static IRelationalDatabaseTable CreateTable(
        Identifier tableName,
        IReadOnlyCollection<string> columnNames,
        IReadOnlyCollection<string> indexNames,
        IReadOnlyCollection<string> triggerNames)
    {
        var columns = columnNames.Select(CreateColumn).ToList();
        var indexes = indexNames
            .Select(indexName => new DatabaseIndex(indexName, false, [CreateIndexColumn("id")], [], true, Option<string>.None))
            .ToList();
        var triggers = triggerNames
            .Select(triggerName => new DatabaseTrigger(triggerName, "create trigger test", TriggerQueryTiming.After, TriggerEvent.Insert, true))
            .ToList();

        return new RelationalDatabaseTable(
            tableName,
            columns,
            null,
            [],
            [],
            [],
            indexes,
            [],
            triggers
        );
    }

    private static IDatabaseIndexColumn CreateIndexColumn(string columnName)
    {
        var indexColumn = new Mock<IDatabaseIndexColumn>(MockBehavior.Strict);
        indexColumn.Setup(static c => c.DependentColumns).Returns([CreateColumn(columnName)]);
        return indexColumn.Object;
    }

    private static IRelationalDatabaseTable CreateFillerTable() => CreateTable("tbl_filler", FillerWords);

    // Spells a word across the given number of distinct names, which is what the rule counts when
    // it decides that a spelling is the one the schema agrees on.
    private static IRelationalDatabaseTable CreateWordSupportTable(Identifier tableName, string word, int nameCount)
    {
        return CreateTable(tableName, ShortPrefixes.Take(nameCount).Select(prefix => prefix + "_" + word).ToArray());
    }

    private static IReadOnlyCollection<IRelationalDatabaseTable> CreateSchema(string supportedWord, int nameCount, params IRelationalDatabaseTable[] tables)
    {
        return
        [
            CreateFillerTable(),
            CreateWordSupportTable("tbl_support", supportedWord, nameCount),
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
    public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseViews(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("views")
        );
    }

    [Test]
    public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseSequences(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequences")
        );
    }

    [Test]
    public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseSynonyms(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonyms")
        );
    }

    [Test]
    public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseRoutines(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routines")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInAColumnName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = CreateSchema("address", 15, CreateTable(suspectTableName, "shipping_adress"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The column name 'shipping_adress' in the table {Name(suspectTableName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInAColumnName_ReportsAgainstTheTableHoldingIt()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = CreateSchema("address", 15, CreateTable(suspectTableName, "shipping_adress"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().ObjectName.UnwrapSome(), Is.EqualTo(suspectTableName));
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInTheTableName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "adress_book");
        var tables = CreateSchema("address", 15, CreateTable(suspectTableName, "id"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The name of the table {Name(suspectTableName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInAnIndexName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = CreateSchema("address", 15, CreateTable(suspectTableName, ["id"], ["ix_adress"], []));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The index name 'ix_adress' in the table {Name(suspectTableName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenMisspelledWordInATriggerName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = CreateSchema("address", 15, CreateTable(suspectTableName, ["id"], [], ["tr_adress_audit"]));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The trigger name 'tr_adress_audit' in the table {Name(suspectTableName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenASchemaOfThreeTables_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateTable("customers", "customer_id", "home_address", "work_address"),
            CreateTable("suppliers", "supplier_id", "home_address", "postal_address"),
            CreateTable("orders", "order_id", "shipping_adress"),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenASingularAgainstItsPlural_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("orders", 15, CreateTable("tbl_lines", "line_order"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenASingularAgainstItsSibilantPlural_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("addresses", 15, CreateTable("tbl_orders", "shipping_address"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenAnAbbreviationOfACommonWord_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("description", 15, CreateTable("tbl_items", "item_descr"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenShortJargonWords_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);

        // 'rate' is one edit from the widely used 'date' and is kept quiet by the length floor
        // alone, which is what the jargon words alongside it also rely on.
        var tables = CreateSchema("date", 15, CreateTable("tbl_items", "rate", "dt", "qty", "fk", "utm"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenBothRegionalSpellingsInWideUse_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateFillerTable(),
            CreateWordSupportTable("tbl_british", "organisation", 8),
            CreateWordSupportTable("tbl_american", "organization", 8),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    // A lone regional spelling against a common other is reported on purpose: the schema cannot
    // tell the two cases apart, so the message says the word differs rather than that it is wrong.
    [Test]
    public static async Task AnalyseTables_GivenOneRegionalSpellingAgainstACommonOther_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("organisation", 15, CreateTable("tbl_owners", "parent_organization"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().Message, Does.Contain("contains the word 'organization', which does not appear elsewhere in the schema and differs by one character from 'organisation', used in 15 other names"));
    }

    [Test]
    public static async Task AnalyseTables_GivenNamesWithNumericSuffixes_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("address", 15, CreateTable("tbl_orders", "address1", "address2"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenNameWithTwoSuspectWords_ProducesOneMessagePerWord()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectTableName = Identifier.CreateQualifiedIdentifier("main", "orders");
        var tables = new[]
        {
            CreateFillerTable(),
            CreateWordSupportTable("tbl_support", "address", 15),
            CreateWordSupportTable("tbl_stock", "quantity", 15),
            CreateTable(suspectTableName, "adress_quanitty"),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Select(static m => m.Message).ToList(),
            Is.EqualTo(new[]
            {
                $"The column name 'adress_quanitty' in the table {Name(suspectTableName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.",
                $"The column name 'adress_quanitty' in the table {Name(suspectTableName)} contains the word 'quanitty', which does not appear elsewhere in the schema and differs by one character from 'quantity', used in 15 other names. Consider whether this is a misspelling.",
            })
        );
    }

    [Test]
    public static async Task AnalyseTables_GivenNameRepeatingASuspectWord_ProducesOneMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("address", 15, CreateTable("tbl_orders", "adress_adress"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Count.EqualTo(1));
    }

    [Test]
    public static async Task AnalyseTables_GivenSuspectNamesAcrossTables_OrdersMessagesByTable()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = new[]
        {
            CreateFillerTable(),
            CreateWordSupportTable("tbl_support", "address", 15),
            CreateTable("zebra", "shipping_adress"),
            CreateTable("alpha", "billing_adddress"),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages.Select(static m => m.ObjectName.UnwrapSome().LocalName).ToList(),
            Is.EqualTo(new[] { "alpha", "zebra" })
        );
    }

    // Without the camel-case split the two names are single words that appear once each, and
    // neither can be measured against the other. Split, the schema's 'address' is what the
    // misspelling is weighed against.
    [Test]
    public static async Task AnalyseTables_GivenCamelCasedName_SplitsItIntoWords()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var tables = CreateSchema("address", 15, CreateTable("tbl_orders", "ShippingAddress", "ShippingAdress"));

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages.Single().Message, Does.Contain("The column name 'ShippingAdress' in the table"));
    }

    [Test]
    public static async Task AnalyseViews_GivenMisspelledWordInAColumnName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectViewName = Identifier.CreateQualifiedIdentifier("main", "order_summary");
        var views = new[]
        {
            CreateView("vw_filler", FillerWords),
            CreateView("vw_support", ShortPrefixes.Take(15).Select(static p => p + "_address").ToArray()),
            CreateView(suspectViewName, "shipping_adress"),
        };

        var messages = await rule.AnalyseViews(views);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The column name 'shipping_adress' in the view {Name(suspectViewName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseSynonyms_GivenMisspelledWordInASynonymName_ProducesMessage()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var suspectSynonymName = Identifier.CreateQualifiedIdentifier("main", "shipping_adress");
        var synonyms = FillerWords
            .Select(static word => new DatabaseSynonym(word, "target"))
            .Concat(ShortPrefixes.Take(15).Select(static p => new DatabaseSynonym(p + "_address", "target")))
            .Append(new DatabaseSynonym(suspectSynonymName, "target"))
            .ToList();

        var messages = await rule.AnalyseSynonyms(synonyms);

        Assert.That(
            messages.Single().Message,
            Is.EqualTo($"The name of the synonym {Name(suspectSynonymName)} contains the word 'adress', which does not appear elsewhere in the schema and differs by one character from 'address', used in 15 other names. Consider whether this is a misspelling.")
        );
    }

    [Test]
    public static async Task AnalyseRoutines_GivenTooFewRoutinesToFormAVocabulary_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var routines = ShortPrefixes
            .Take(15)
            .Select(static p => new DatabaseRoutine(p + "_address", "definition"))
            .Append(new DatabaseRoutine("shipping_adress", "definition"))
            .ToList();

        var messages = await rule.AnalyseRoutines(routines);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseSequences_GivenTooFewSequencesToFormAVocabulary_ProducesNoMessages()
    {
        var rule = new LikelyMisspelledNameRule(RuleLevel.Error);
        var sequences = ShortPrefixes
            .Take(15)
            .Select(static p => CreateSequence(p + "_address"))
            .Append(CreateSequence("shipping_adress"))
            .ToList();

        var messages = await rule.AnalyseSequences(sequences);

        Assert.That(messages, Is.Empty);
    }

    private static IDatabaseView CreateView(Identifier viewName, params string[] columnNames)
    {
        return new DatabaseView(viewName, "select 1", columnNames.Select(CreateColumn).ToList());
    }

    private static IDatabaseSequence CreateSequence(Identifier sequenceName)
    {
        return new DatabaseSequence(
            sequenceName,
            new ColumnDataType("integer", DataType.Integer, "integer", typeof(int), false, 0, Option<INumericPrecision>.None, Option<Identifier>.None),
            1,
            1,
            Option<decimal>.None,
            Option<decimal>.None,
            true,
            SequenceCacheMode.Unknown,
            Option<int>.None,
            false
        );
    }
}
