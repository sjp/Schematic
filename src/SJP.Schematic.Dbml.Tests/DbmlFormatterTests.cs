using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dbml.Tests;

internal static class DbmlFormatterTests
{
    [Test]
    public static void RenderTables_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(() => new DbmlFormatter().RenderTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void RenderTables_GivenEmptyTables_ReturnsEmptyString()
    {
        var formatter = new DbmlFormatter();
        var tables = Array.Empty<IRelationalDatabaseTable>();

        var result = formatter.RenderTables(tables);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void RenderTables_GivenTableWithCompositePrimaryKey_RendersPrimaryKeyAsIndex()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("test_table_pk"), DatabaseKeyType.Primary, columns.Take(2).ToList(), true);
        var table = CreateTable(columns, primaryKey, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(CompositePrimaryKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableWithCompositeUniqueKey_RendersUniqueKeyAsIndex()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, columns.Take(2).ToList(), true);
        var table = CreateTable(columns, Option<IDatabaseKey>.None, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(CompositeUniqueKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableWithUnnamedCompositePrimaryKey_RendersPrimaryKeyWithoutName()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var primaryKey = new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Primary, columns.Take(2).ToList(), true);
        var table = CreateTable(columns, primaryKey, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(UnnamedCompositePrimaryKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenCompositeKeyWithBackingUniqueIndex_DoesNotRenderIndexTwice()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var keyColumns = columns.Take(2).ToList();
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, keyColumns, true);
        var backingIndex = new DatabaseIndex(
            "test_table_uk",
            true,
            keyColumns.ConvertAll(c => (IDatabaseIndexColumn)new DatabaseIndexColumn(c.Name.LocalName, c, IndexColumnOrder.Ascending)),
            [],
            true,
            Option<string>.None
        );
        var table = CreateTable(columns, Option<IDatabaseKey>.None, [uniqueKey], [backingIndex]);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(CompositeUniqueKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenNonUniqueIndexMatchingCompositeKeyColumns_RendersBothEntries()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var keyColumns = columns.Take(2).ToList();
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, keyColumns, true);
        var index = new DatabaseIndex(
            "ix_test_table",
            false,
            keyColumns.ConvertAll(c => (IDatabaseIndexColumn)new DatabaseIndexColumn(c.Name.LocalName, c, IndexColumnOrder.Ascending)),
            [],
            true,
            Option<string>.None
        );
        var table = CreateTable(columns, Option<IDatabaseKey>.None, [uniqueKey], [index]);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(CompositeUniqueKeyWithIndexDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableAndColumnNamesRequiringQuoting_QuotesNames()
    {
        var columns = CreateColumns("Order ID", "Unit Price");
        var table = CreateTable("Order Details", columns, Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(QuotedNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenDialectQuotedNames_RemovesOnlyEnclosingQuoting()
    {
        var columns = CreateColumns("[Order ID]", "\"Unit Price\"");
        var table = CreateTable("[Order Details]", columns, Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(QuotedNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenNamesWithEmbeddedQuotingCharacters_PreservesAndEscapesThem()
    {
        var columns = CreateColumns("a\"b", "c[d]e");
        var table = CreateTable("first'second", columns, Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(EmbeddedQuotingCharactersDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTypeNamesRequiringQuoting_QuotesOnlyUnsafeTypes()
    {
        var columns = new List<IDatabaseColumn>
        {
            CreateColumn("created", "timestamp with time zone"),
            CreateColumn("ratio", "double precision"),
            CreateColumn("label", "character varying(50)"),
            CreateColumn("amount", "numeric(10, 2)"),
        };
        var table = CreateTable("test_table", columns, Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(QuotedTypeNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenExpressionIndexColumns_RendersExpressionsInBackticks()
    {
        var columns = CreateColumns("email", "first_name");
        var functionalIndex = new DatabaseIndex(
            "ix_test_table_email",
            false,
            [new DatabaseIndexColumn("lower(email)", columns[0], IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );
        var mixedIndex = new DatabaseIndex(
            "ix_test_table_mixed",
            false,
            [
                new DatabaseIndexColumn("first_name", columns[1], IndexColumnOrder.Ascending),
                new DatabaseIndexColumn("lower('x' || email)", columns[0], IndexColumnOrder.Ascending),
            ],
            [],
            true,
            Option<string>.None
        );
        var table = CreateTable("test_table", columns, Option<IDatabaseKey>.None, [], [functionalIndex, mixedIndex]);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(ExpressionIndexDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenIndexAndKeyNamesContainingApostrophes_EscapesNames()
    {
        var columns = CreateColumns("first_name", "last_name");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("o'brien pk"), DatabaseKeyType.Primary, columns, true);
        var index = new DatabaseIndex(
            "o'brien ix",
            false,
            [new DatabaseIndexColumn("first_name", columns[0], IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );
        var table = CreateTable("test_table", columns, primaryKey, [], [index]);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(EscapedIndexNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenForeignKeyWithNamesRequiringQuoting_QuotesRefNames()
    {
        var parentColumns = CreateColumns("Order ID");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("orders_pk"), DatabaseKeyType.Primary, parentColumns, true);
        var parentTable = CreateTable("Order Details", parentColumns, parentKey, [], []);

        var childColumns = CreateColumns("Order ID");
        var childKey = new DatabaseKey(Option<Identifier>.Some("lines_fk"), DatabaseKeyType.Foreign, childColumns, true);
        var relationalKey = new DatabaseRelationalKey(
            "Order Lines",
            childKey,
            "Order Details",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable("Order Lines", childColumns, Option<IDatabaseKey>.None, [], [relationalKey], [], [], [], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(QuotedForeignKeyDbml).IgnoreLineEndingFormat);
    }

    private static List<IDatabaseColumn> CreateColumns(params string[] columnNames)
    {
        return columnNames
            .Select(name => CreateColumn(name, "text"))
            .ToList();
    }

    private static IDatabaseColumn CreateColumn(Identifier columnName, string typeDefinition)
    {
        var columnType = new ColumnDataType(
            "text",
            DataType.String,
            typeDefinition,
            typeof(string),
            false,
            255,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        return new DatabaseColumn(columnName, columnType, false, Option<string>.None, Option<IAutoIncrement>.None);
    }

    private static IRelationalDatabaseTable CreateTable(
        IReadOnlyList<IDatabaseColumn> columns,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys,
        IReadOnlyCollection<IDatabaseIndex> indexes
    ) => CreateTable("test_table", columns, primaryKey, uniqueKeys, indexes);

    private static IRelationalDatabaseTable CreateTable(
        Identifier tableName,
        IReadOnlyList<IDatabaseColumn> columns,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys,
        IReadOnlyCollection<IDatabaseIndex> indexes
    ) => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, [], [], indexes, [], []);

    private const string CompositePrimaryKeyDbml = """
Table test_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [name: 'test_table_pk', pk]
    }
}
""";

    private const string CompositeUniqueKeyDbml = """
Table test_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [name: 'test_table_uk', unique]
    }
}
""";

    private const string UnnamedCompositePrimaryKeyDbml = """
Table test_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [pk]
    }
}
""";

    private const string CompositeUniqueKeyWithIndexDbml = """
Table test_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [name: 'test_table_uk', unique]
        (first_name, last_name) [name: 'ix_test_table']
    }
}
""";

    private const string QuotedNamesDbml = """
Table "Order Details" {
    "Order ID" text [not null]
    "Unit Price" text [not null]
}
""";

    private const string EmbeddedQuotingCharactersDbml = """"
Table "first'second" {
    "a\"b" text [not null]
    "c[d]e" text [not null]
}
"""";

    private const string QuotedTypeNamesDbml = """
Table test_table {
    created "timestamp with time zone" [not null]
    ratio "double precision" [not null]
    label "character varying(50)" [not null]
    amount numeric(10, 2) [not null]
}
""";

    private const string ExpressionIndexDbml = """
Table test_table {
    email text [not null]
    first_name text [not null]

    Indexes {
        `lower(email)` [name: 'ix_test_table_email']
        (first_name, `lower('x' || email)`) [name: 'ix_test_table_mixed']
    }
}
""";

    private const string EscapedIndexNamesDbml = """"
Table test_table {
    first_name text [not null]
    last_name text [not null]

    Indexes {
        (first_name, last_name) [name: 'o\'brien pk', pk]
        first_name [name: 'o\'brien ix']
    }
}
"""";

    private const string QuotedForeignKeyDbml = """
Table "Order Details" {
    "Order ID" text [not null, primary key]
}

Table "Order Lines" {
    "Order ID" text [not null]
}

Ref: "Order Lines"."Order ID" > "Order Details"."Order ID"
""";
}
