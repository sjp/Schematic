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

    private static List<IDatabaseColumn> CreateColumns(params string[] columnNames)
    {
        var columnType = new ColumnDataType(
            "text",
            DataType.String,
            "text",
            typeof(string),
            false,
            255,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        return columnNames
            .Select(name => (IDatabaseColumn)new DatabaseColumn(name, columnType, false, Option<string>.None, Option<IAutoIncrement>.None))
            .ToList();
    }

    private static IRelationalDatabaseTable CreateTable(
        IReadOnlyList<IDatabaseColumn> columns,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys,
        IReadOnlyCollection<IDatabaseIndex> indexes
    ) => new RelationalDatabaseTable("test_table", columns, primaryKey, uniqueKeys, [], [], indexes, [], []);

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
}
