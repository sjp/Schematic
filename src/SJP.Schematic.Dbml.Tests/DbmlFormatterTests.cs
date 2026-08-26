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
    public static void RenderTables_GivenTablesWithNullValue_ThrowsArgumentNullException()
    {
        var tables = new IRelationalDatabaseTable[] { null };

        Assert.That(() => new DbmlFormatter().RenderTables(tables), Throws.ArgumentNullException);
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
    public static void RenderTables_GivenNullableColumn_RendersNullOption()
    {
        var columns = new List<IDatabaseColumn>
        {
            CreateColumn("first_name", "text", Option<string>.None, true, Option<IAutoIncrement>.None),
            CreateColumn("last_name", "text"),
        };
        var table = CreateTable(columns, Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(NullableColumnDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenAutoIncrementPrimaryKeyColumnWithDefault_RendersAllColumnOptions()
    {
        var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(1, 1));
        var column = CreateColumn("id", "text", Option<string>.Some("0"), false, autoIncrement);
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("test_table_pk"), DatabaseKeyType.Primary, [column], true);
        var table = CreateTable([column], primaryKey, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(AutoIncrementColumnDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenSingleColumnUniqueKey_RendersUniqueColumnOptionWithoutIndexEntry()
    {
        var columns = CreateColumns("first_name", "last_name");
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, [columns[0]], true);
        var table = CreateTable(columns, Option<IDatabaseKey>.None, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(SingleColumnUniqueKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenColumnInSingleColumnPrimaryAndUniqueKeys_RendersOnlyPrimaryKeyOption()
    {
        var columns = CreateColumns("id");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("test_table_pk"), DatabaseKeyType.Primary, columns, true);
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, columns, true);
        var table = CreateTable(columns, primaryKey, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(PrimaryAndUniqueKeyColumnDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableWithCompositePrimaryAndUniqueKeys_RendersPrimaryKeyBeforeUniqueKey()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("test_table_pk"), DatabaseKeyType.Primary, columns.Take(2).ToList(), true);
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("test_table_uk"), DatabaseKeyType.Unique, columns.Skip(1).ToList(), true);
        var table = CreateTable(columns, primaryKey, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([table]);

        Assert.That(result, Is.EqualTo(CompositePrimaryAndUniqueKeyDbml).IgnoreLineEndingFormat);
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

    [TestCase("0", "0")]
    [TestCase("-1", "-1")]
    [TestCase("12.50", "12.50")]
    [TestCase("1.5e-3", "1.5e-3")]
    [TestCase("(0)", "0")]
    [TestCase("((42))", "42")]
    [TestCase("NULL", "null")]
    [TestCase("True", "true")]
    [TestCase("false", "false")]
    [TestCase("'test'", "'test'")]
    [TestCase("N'test'", "'test'")]
    [TestCase("('test')", "'test'")]
    [TestCase("''", "''")]
    [TestCase("   ", "''")]
    [TestCase("'o''brien'", @"'o\'brien'")]
    [TestCase(@"'a\b'", @"'a\\b'")]
    [TestCase("\'a \"b\" c\'", "'a \"b\" c'")]
    [TestCase("CURRENT_TIMESTAMP", "`CURRENT_TIMESTAMP`")]
    [TestCase("now()", "`now()`")]
    [TestCase("(getdate())", "`getdate()`")]
    [TestCase("'a' || 'b'", "`'a' || 'b'`")]
    [TestCase("(a) + (b)", "`(a) + (b)`")]
    [TestCase("(', ')", "', '")]
    [TestCase("`quoted`", "'`quoted`'")]
    public static void RenderTables_GivenColumnWithDefaultValue_RendersClassifiedDefault(string defaultValue, string expectedDefault)
    {
        var column = CreateColumn("test_column", "text", Option<string>.Some(defaultValue));
        var table = CreateTable("test_table", [column], Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([table]);

        var expected = "Table test_table {\n    test_column text [not null, default: " + expectedDefault + "]\n}";
        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
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

    [Test]
    public static void RenderTables_GivenChildKeyMatchingQuotedUniqueIndexExpression_RendersOneToOneRelationship()
    {
        var columns = CreateColumns("order_id");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, columns, true);
        var (parentTable, relationalKey) = CreateParentTableWithRelationalKey(childKey);
        var uniqueIndex = new DatabaseIndex(
            "ix_child_table_order_id",
            true,
            [new ExpressionIndexColumn("\"order_id\"")],
            [],
            true,
            Option<string>.None
        );
        var childTable = CreateChildTable(columns, relationalKey, [], [uniqueIndex]);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(QuotedUniqueIndexExpressionForeignKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenChildKeyContainingUniqueKeyColumns_RendersOneToOneRelationship()
    {
        var columns = CreateColumns("first_name", "last_name", "comment");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, columns, true);
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("child_table_uk"), DatabaseKeyType.Unique, columns.Take(2).ToList(), true);
        var (parentTable, relationalKey) = CreateParentTableWithRelationalKey(childKey);
        var childTable = CreateChildTable(columns, relationalKey, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(UniqueKeySubsetForeignKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenChildKeyMatchingUniqueKeyInDifferentColumnOrder_RendersOneToOneRelationship()
    {
        var columns = CreateColumns("first_name", "last_name");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, [columns[1], columns[0]], true);
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("child_table_uk"), DatabaseKeyType.Unique, columns, true);
        var (parentTable, relationalKey) = CreateParentTableWithRelationalKey(childKey);
        var childTable = CreateChildTable(columns, relationalKey, [uniqueKey], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(ReorderedUniqueKeyForeignKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenChildKeyMatchingFunctionalUniqueIndexColumns_RendersManyToOneRelationship()
    {
        var columns = CreateColumns("email");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, columns, true);
        var uniqueIndex = new DatabaseIndex(
            "ix_child_table_email",
            true,
            [new DatabaseIndexColumn("lower(email)", columns[0], IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );
        var (parentTable, relationalKey) = CreateParentTableWithRelationalKey(childKey);
        var childTable = CreateChildTable(columns, relationalKey, [], [uniqueIndex]);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(FunctionalUniqueIndexForeignKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenForeignKeyToTableOutsideRenderedSet_DoesNotRenderRef()
    {
        var columns = CreateColumns("order_id");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, columns, true);
        var (_, relationalKey) = CreateParentTableWithRelationalKey(childKey);
        var childTable = CreateChildTable(columns, relationalKey, [], []);

        var result = new DbmlFormatter().RenderTables([childTable]);

        Assert.That(result, Is.EqualTo(UnrenderedParentForeignKeyDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenForeignKeysInsideAndOutsideRenderedSet_RendersOnlyRefsToRenderedTables()
    {
        var columns = CreateColumns("order_id", "customer_id");
        var renderedChildKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, [columns[0]], true);
        var (parentTable, renderedRelationalKey) = CreateParentTableWithRelationalKey(renderedChildKey);

        var missingChildKey = new DatabaseKey(Option<Identifier>.Some("child_table_customer_fk"), DatabaseKeyType.Foreign, [columns[1]], true);
        var missingParentKey = new DatabaseKey(Option<Identifier>.Some("missing_table_pk"), DatabaseKeyType.Primary, CreateColumns("customer_id"), true);
        var missingRelationalKey = new DatabaseRelationalKey(
            "child_table",
            missingChildKey,
            "missing_table",
            missingParentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var childTable = new RelationalDatabaseTable(
            "child_table",
            columns,
            Option<IDatabaseKey>.None,
            [],
            [renderedRelationalKey, missingRelationalKey],
            [],
            [],
            [],
            []
        );

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(PartiallyRenderedForeignKeysDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenSchemaQualifiedParentTableOutsideRenderedSet_DoesNotRenderRef()
    {
        var columns = CreateColumns("order_id");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, columns, true);
        var parentColumns = CreateColumns("order_id");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("parent_table_pk"), DatabaseKeyType.Primary, parentColumns, true);
        var parentTable = CreateTable(Identifier.CreateQualifiedIdentifier("a_b", "c"), parentColumns, parentKey, [], []);

        var childTableName = Identifier.CreateQualifiedIdentifier("a", "child_table");
        var relationalKey = new DatabaseRelationalKey(
            childTableName,
            childKey,
            Identifier.CreateQualifiedIdentifier("a", "b_c"),
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable(childTableName, columns, Option<IDatabaseKey>.None, [], [relationalKey], [], [], [], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(UnrenderedSchemaQualifiedParentDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenSchemaQualifiedTableNames_RendersSchemaQualifiedDbmlNames()
    {
        var parentColumns = CreateColumns("Order ID");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("orders_pk"), DatabaseKeyType.Primary, parentColumns, true);
        var parentTableName = Identifier.CreateQualifiedIdentifier("sales", "orders");
        var parentTable = CreateTable(parentTableName, parentColumns, parentKey, [], []);

        var childColumns = CreateColumns("Order ID");
        var childKey = new DatabaseKey(Option<Identifier>.Some("lines_fk"), DatabaseKeyType.Foreign, childColumns, true);
        var childTableName = Identifier.CreateQualifiedIdentifier("archive schema", "order lines");
        var relationalKey = new DatabaseRelationalKey(
            childTableName,
            childKey,
            parentTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable(childTableName, childColumns, Option<IDatabaseKey>.None, [], [relationalKey], [], [], [], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(SchemaQualifiedNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableNamesSharingAFlattenedForm_RendersDistinctTablesAndRefs()
    {
        var parentColumns = CreateColumns("order_id");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("parent_table_pk"), DatabaseKeyType.Primary, parentColumns, true);
        var parentTableName = Identifier.CreateQualifiedIdentifier("a_b", "c");
        var parentTable = CreateTable(parentTableName, parentColumns, parentKey, [], []);

        var childColumns = CreateColumns("order_id");
        var childKey = new DatabaseKey(Option<Identifier>.Some("child_table_fk"), DatabaseKeyType.Foreign, childColumns, true);
        var childTableName = Identifier.CreateQualifiedIdentifier("a", "b_c");
        var relationalKey = new DatabaseRelationalKey(
            childTableName,
            childKey,
            parentTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable(childTableName, childColumns, Option<IDatabaseKey>.None, [], [relationalKey], [], [], [], []);

        var result = new DbmlFormatter().RenderTables([parentTable, childTable]);

        Assert.That(result, Is.EqualTo(CollidingFlattenedNamesDbml).IgnoreLineEndingFormat);
    }

    [Test]
    public static void RenderTables_GivenTableNamesQualifiedBeyondSchema_FoldsLeadingPartsIntoSchemaName()
    {
        var columns = CreateColumns("order_id");
        var firstTable = CreateTable(Identifier.CreateQualifiedIdentifier("live", "sales", "orders"), columns, Option<IDatabaseKey>.None, [], []);
        var secondTable = CreateTable(Identifier.CreateQualifiedIdentifier("remote_server", "backup", "sales", "orders"), CreateColumns("order_id"), Option<IDatabaseKey>.None, [], []);

        var result = new DbmlFormatter().RenderTables([firstTable, secondTable]);

        Assert.That(result, Is.EqualTo(FullyQualifiedNamesDbml).IgnoreLineEndingFormat);
    }

    private static (IRelationalDatabaseTable ParentTable, IDatabaseRelationalKey RelationalKey) CreateParentTableWithRelationalKey(IDatabaseKey childKey)
    {
        var parentColumns = CreateColumns(childKey.Columns.Select(static c => c.Name.LocalName).ToArray());
        var parentKey = new DatabaseKey(Option<Identifier>.Some("parent_table_pk"), DatabaseKeyType.Primary, parentColumns, true);
        var parentTable = CreateTable("parent_table", parentColumns, parentKey, [], []);
        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            childKey,
            "parent_table",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        return (parentTable, relationalKey);
    }

    private static IRelationalDatabaseTable CreateChildTable(
        IReadOnlyList<IDatabaseColumn> columns,
        IDatabaseRelationalKey relationalKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys,
        IReadOnlyCollection<IDatabaseIndex> indexes
    ) => new RelationalDatabaseTable("child_table", columns, Option<IDatabaseKey>.None, uniqueKeys, [relationalKey], [], indexes, [], []);

    private sealed class ExpressionIndexColumn(string expression) : IDatabaseIndexColumn
    {
        public string Expression { get; } = expression;

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; } = [];

        public IndexColumnOrder Order { get; } = IndexColumnOrder.Ascending;
    }

    private static List<IDatabaseColumn> CreateColumns(params string[] columnNames)
    {
        return columnNames
            .Select(name => CreateColumn(name, "text"))
            .ToList();
    }

    private static IDatabaseColumn CreateColumn(Identifier columnName, string typeDefinition)
        => CreateColumn(columnName, typeDefinition, Option<string>.None);

    private static IDatabaseColumn CreateColumn(Identifier columnName, string typeDefinition, Option<string> defaultValue)
        => CreateColumn(columnName, typeDefinition, defaultValue, false, Option<IAutoIncrement>.None);

    private static IDatabaseColumn CreateColumn(
        Identifier columnName,
        string typeDefinition,
        Option<string> defaultValue,
        bool isNullable,
        Option<IAutoIncrement> autoIncrement
    )
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

        return new DatabaseColumn(
            columnName,
            columnType,
            isNullable,
            defaultValue,
            autoIncrement
        );
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

    private const string NullableColumnDbml = """
Table test_table {
    first_name text [null]
    last_name text [not null]
}
""";

    private const string AutoIncrementColumnDbml = """
Table test_table {
    id text [not null, increment, primary key, default: 0]
}
""";

    private const string SingleColumnUniqueKeyDbml = """
Table test_table {
    first_name text [not null, unique]
    last_name text [not null]
}
""";

    private const string PrimaryAndUniqueKeyColumnDbml = """
Table test_table {
    id text [not null, primary key]
}
""";

    private const string CompositePrimaryAndUniqueKeyDbml = """
Table test_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [name: 'test_table_pk', pk]
        (last_name, comment) [name: 'test_table_uk', unique]
    }
}
""";

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

    private const string QuotedUniqueIndexExpressionForeignKeyDbml = """
Table parent_table {
    order_id text [not null, primary key]
}

Table child_table {
    order_id text [not null]

    Indexes {
        `"order_id"` [name: 'ix_child_table_order_id', unique]
    }
}

Ref: child_table.order_id - parent_table.order_id
""";

    private const string UniqueKeySubsetForeignKeyDbml = """
Table parent_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name, comment) [name: 'parent_table_pk', pk]
    }
}

Table child_table {
    first_name text [not null]
    last_name text [not null]
    comment text [not null]

    Indexes {
        (first_name, last_name) [name: 'child_table_uk', unique]
    }
}

Ref: child_table.(first_name, last_name, comment) - parent_table.(first_name, last_name, comment)
""";

    private const string ReorderedUniqueKeyForeignKeyDbml = """
Table parent_table {
    last_name text [not null]
    first_name text [not null]

    Indexes {
        (last_name, first_name) [name: 'parent_table_pk', pk]
    }
}

Table child_table {
    first_name text [not null]
    last_name text [not null]

    Indexes {
        (first_name, last_name) [name: 'child_table_uk', unique]
    }
}

Ref: child_table.(last_name, first_name) - parent_table.(last_name, first_name)
""";

    private const string FunctionalUniqueIndexForeignKeyDbml = """
Table parent_table {
    email text [not null, primary key]
}

Table child_table {
    email text [not null]

    Indexes {
        `lower(email)` [name: 'ix_child_table_email', unique]
    }
}

Ref: child_table.email > parent_table.email
""";

    private const string UnrenderedParentForeignKeyDbml = """
Table child_table {
    order_id text [not null]
}
""";

    private const string PartiallyRenderedForeignKeysDbml = """
Table parent_table {
    order_id text [not null, primary key]
}

Table child_table {
    order_id text [not null]
    customer_id text [not null]
}

Ref: child_table.order_id > parent_table.order_id
""";

    private const string UnrenderedSchemaQualifiedParentDbml = """
Table a_b.c {
    order_id text [not null, primary key]
}

Table a.child_table {
    order_id text [not null]
}
""";

    private const string SchemaQualifiedNamesDbml = """
Table sales.orders {
    "Order ID" text [not null, primary key]
}

Table "archive schema"."order lines" {
    "Order ID" text [not null]
}

Ref: "archive schema"."order lines"."Order ID" > sales.orders."Order ID"
""";

    private const string CollidingFlattenedNamesDbml = """
Table a_b.c {
    order_id text [not null, primary key]
}

Table a.b_c {
    order_id text [not null]
}

Ref: a.b_c.order_id > a_b.c.order_id
""";

    private const string FullyQualifiedNamesDbml = """
Table "live.sales".orders {
    order_id text [not null]
}

Table "remote_server.backup.sales".orders {
    order_id text [not null]
}
""";

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
