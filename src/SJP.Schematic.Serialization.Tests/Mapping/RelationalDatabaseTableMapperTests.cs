using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class RelationalDatabaseTableMapperTests
{
    private static DatabaseColumn CreateColumn(string columnName)
    {
        return new DatabaseColumn(
            columnName,
            new ColumnDataType("int", DataType.Integer, "int", typeof(int), false, 4, Option<INumericPrecision>.None, Option<Identifier>.None),
            false,
            Option<IDatabaseDefaultValue>.None,
            Option<IAutoIncrement>.None
        );
    }

    [Test]
    public static void Map_GivenTableWithStorageMetadata_RoundTrips()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var partitionColumn = CreateColumn("created_on");
        var table = new RelationalDatabaseTable(
            "test_table",
            [partitionColumn, CreateColumn("id")],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            [],
            TableKind.PartitionParent,
            Option<ITablePartitioning>.Some(new TablePartitioning("RANGE", [partitionColumn], ["test_table_2025", "test_table_2026"])),
            Option<ITableSystemVersioning>.Some(new TableSystemVersioning("test_table_history", "valid_from", "valid_to")),
            false,
            Option<Identifier>.Some("utf8mb4_general_ci")
        );

        var result = mapper.Map(mapper.Map(table));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Kind, Is.EqualTo(TableKind.PartitionParent));
            Assert.That(result.IsLogged, Is.False);
            Assert.That(result.Collation.UnwrapSome().LocalName, Is.EqualTo("utf8mb4_general_ci"));

            var partitioning = result.Partitioning.UnwrapSome();
            Assert.That(partitioning.Strategy, Is.EqualTo("RANGE"));
            Assert.That(partitioning.Partitions.Select(static p => p.LocalName), Is.EqualTo(new[] { "test_table_2025", "test_table_2026" }));

            // a partitioning key refers to the table's own columns, not to copies of them
            Assert.That(partitioning.Columns, Has.Count.EqualTo(1));
            Assert.That(partitioning.Columns[0], Is.SameAs(result.Columns[0]));

            var systemVersioning = result.SystemVersioning.UnwrapSome();
            Assert.That(systemVersioning.HistoryTable.LocalName, Is.EqualTo("test_table_history"));
            Assert.That(systemVersioning.PeriodStartColumn.LocalName, Is.EqualTo("valid_from"));
            Assert.That(systemVersioning.PeriodEndColumn.LocalName, Is.EqualTo("valid_to"));
        }
    }

    [Test]
    public static void Map_GivenDtoWithoutStorageMetadata_ReturnsOrdinaryLoggedTable()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var dto = new Dto.RelationalDatabaseTable
        {
            TableName = new Dto.Identifier { LocalName = "test_table" },
            Columns = [],
            Checks = [],
            Indexes = [],
            UniqueKeys = [],
            ParentKeys = [],
            ChildKeys = [],
            Triggers = [],
        };

        var result = mapper.Map(dto);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(result.Partitioning, OptionIs.None);
            Assert.That(result.SystemVersioning, OptionIs.None);
            Assert.That(result.IsLogged, Is.True);
            Assert.That(result.Collation, OptionIs.None);
        }
    }

    [Test]
    public static void Map_GivenTableWithKeysAndIndexes_ReturnsKeyAndIndexColumnsAsTableColumns()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var idColumn = CreateColumn("id");
        var codeColumn = CreateColumn("code");
        var parentIdColumn = CreateColumn("parent_id");

        var primaryKeyIndex = new DatabaseIndex("pk_index", true, [new DatabaseIndexColumn("id", idColumn, IndexColumnOrder.Ascending)], [], true, Option<string>.None);
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk"), DatabaseKeyType.Primary, [idColumn], true, Option<IDatabaseIndex>.Some(primaryKeyIndex));
        var uniqueKey = new DatabaseKey(Option<Identifier>.Some("uk"), DatabaseKeyType.Unique, [codeColumn], true);
        var index = new DatabaseIndex("ix", false, [new DatabaseIndexColumn("parent_id", parentIdColumn, IndexColumnOrder.Ascending)], [codeColumn], true, Option<string>.None);

        // a self-referencing foreign key, so that both of its sides belong to the mapped table
        var foreignKey = new DatabaseRelationalKey(
            "test_table",
            new DatabaseKey(Option<Identifier>.Some("fk"), DatabaseKeyType.Foreign, [parentIdColumn], true),
            "test_table",
            primaryKey,
            ReferentialAction.SetNull,
            ReferentialAction.NoAction,
            ForeignKeyMatchType.Simple,
            [parentIdColumn]
        );

        var table = new RelationalDatabaseTable(
            "test_table",
            [idColumn, codeColumn, parentIdColumn],
            Option<IDatabaseKey>.Some(primaryKey),
            [uniqueKey],
            [foreignKey],
            [foreignKey],
            [index],
            [],
            []
        );

        var result = mapper.Map(mapper.Map(table));

        var resultId = result.Columns[0];
        var resultCode = result.Columns[1];
        var resultParentId = result.Columns[2];
        var resultPrimaryKey = result.PrimaryKey.UnwrapSome();
        var resultIndex = result.Indexes.Single();
        var resultParentKey = result.ParentKeys.Single();
        var resultChildKey = result.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(resultPrimaryKey.Columns.Single(), Is.SameAs(resultId));
            Assert.That(resultPrimaryKey.BackingIndex.UnwrapSome().Columns.Single().DependentColumns.Single(), Is.SameAs(resultId));
            Assert.That(result.UniqueKeys.Single().Columns.Single(), Is.SameAs(resultCode));
            Assert.That(resultIndex.Columns.Single().DependentColumns.Single(), Is.SameAs(resultParentId));
            Assert.That(resultIndex.IncludedColumns.Single(), Is.SameAs(resultCode));
            Assert.That(resultParentKey.ChildKey.Columns.Single(), Is.SameAs(resultParentId));
            Assert.That(resultParentKey.ParentKey.Columns.Single(), Is.SameAs(resultId));
            Assert.That(resultParentKey.SetNullColumns.Single(), Is.SameAs(resultParentId));
            Assert.That(resultChildKey.ChildKey.Columns.Single(), Is.SameAs(resultParentId));
            Assert.That(resultChildKey.ParentKey.Columns.Single(), Is.SameAs(resultId));
        }
    }

    [Test]
    public static void Map_GivenForeignKeyToAnotherTable_ReturnsOtherTableColumnsAsCopies()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var parentIdColumn = CreateColumn("parent_id");
        var otherTableIdColumn = CreateColumn("id");
        var foreignKey = new DatabaseRelationalKey(
            "test_table",
            new DatabaseKey(Option<Identifier>.Some("fk"), DatabaseKeyType.Foreign, [parentIdColumn], true),
            "parent_table",
            new DatabaseKey(Option<Identifier>.Some("pk"), DatabaseKeyType.Primary, [otherTableIdColumn], true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var table = new RelationalDatabaseTable("test_table", [parentIdColumn], Option<IDatabaseKey>.None, [], [foreignKey], [], [], [], []);

        var result = mapper.Map(mapper.Map(table));

        var resultParentKey = result.ParentKeys.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(resultParentKey.ChildKey.Columns.Single(), Is.SameAs(result.Columns[0]));
            Assert.That(resultParentKey.ParentKey.Columns.Single().Name.LocalName, Is.EqualTo("id"));
        }
    }

    [Test]
    public static void Map_GivenKeyColumnMissingFromTableColumns_ReturnsKeyColumnCopy()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var idColumn = CreateColumn("id");
        var hiddenColumn = CreateColumn("hidden_id");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk"), DatabaseKeyType.Primary, [hiddenColumn], true);
        var table = new RelationalDatabaseTable("test_table", [idColumn], Option<IDatabaseKey>.Some(primaryKey), [], [], [], [], [], []);

        var result = mapper.Map(mapper.Map(table));

        var keyColumn = result.PrimaryKey.UnwrapSome().Columns.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(keyColumn.Name.LocalName, Is.EqualTo("hidden_id"));
            Assert.That(keyColumn, Is.Not.SameAs(result.Columns[0]));
        }
    }

    [Test]
    public static void Map_GivenColumnNamesDifferingOnlyByCase_ReturnsKeyColumnWithExactName()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var lowerColumn = CreateColumn("a");
        var upperColumn = CreateColumn("A");
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk"), DatabaseKeyType.Primary, [upperColumn], true);
        var table = new RelationalDatabaseTable("test_table", [lowerColumn, upperColumn], Option<IDatabaseKey>.Some(primaryKey), [], [], [], [], [], []);

        var result = mapper.Map(mapper.Map(table));

        Assert.That(result.PrimaryKey.UnwrapSome().Columns.Single(), Is.SameAs(result.Columns[1]));
    }

    [Test]
    public static void Map_GivenTableWithKeysAndIndexes_ReusesSerializedColumns()
    {
        var mapper = new RelationalDatabaseTableMapper();

        var idColumn = CreateColumn("id");
        var index = new DatabaseIndex("ix", true, [new DatabaseIndexColumn("id", idColumn, IndexColumnOrder.Ascending)], [], true, Option<string>.None);
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk"), DatabaseKeyType.Primary, [idColumn], true, Option<IDatabaseIndex>.Some(index));
        var table = new RelationalDatabaseTable("test_table", [idColumn], Option<IDatabaseKey>.Some(primaryKey), [], [], [], [index], [], []);

        var result = mapper.Map(table);

        var serializedColumn = result.Columns.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PrimaryKey!.Columns.Single(), Is.SameAs(serializedColumn));
            Assert.That(result.PrimaryKey.BackingIndex, Is.SameAs(result.Indexes.Single()));
            Assert.That(result.Indexes.Single().Columns.Single().DependentColumns.Single(), Is.SameAs(serializedColumn));
        }
    }
}
