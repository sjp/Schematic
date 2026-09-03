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
}
