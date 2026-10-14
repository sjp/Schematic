using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

/// <summary>
/// Index columns that the catalog reports in an unusual way: every column of a columnstore index is marked
/// as included, and a partitioning column that SQL Server adds to an aligned index by itself looks like a key.
/// </summary>
internal sealed class SqlServerRelationalDatabaseTableIndexColumnTests : SqlServerTest
{
    private IRelationalDatabaseTableProvider TableProvider => new SqlServerRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create table index_column_columnstore_1 (test_column_1 int, test_column_2 int, test_column_3 int)",
            "create nonclustered columnstore index ix_index_column_columnstore_1 on index_column_columnstore_1 (test_column_3, test_column_1)",
            "create table index_column_columnstore_2 (test_column_1 int, test_column_2 int)",
            "create clustered columnstore index ix_index_column_columnstore_2 on index_column_columnstore_2"
        );
        if (DatabaseProvider.SupportsOrderedColumnStoreIndex())
        {
            await ExecuteBatchAsync(
                "create table index_column_columnstore_3 (test_column_1 int, test_column_2 int, test_column_3 int)",
                "create clustered columnstore index ix_index_column_columnstore_3 on index_column_columnstore_3 order (test_column_3, test_column_2)"
            );
        }

        await ExecuteBatchAsync(
            "create partition function pf_index_column_1 (int) as range left for values (100)",
            "create partition scheme ps_index_column_1 as partition pf_index_column_1 all to ([PRIMARY])"
        );
        await ExecuteBatchAsync(
            "create table index_column_partitioned_1 (part_key int not null, test_column_1 int, test_column_2 int) on ps_index_column_1 (part_key)",
            "create nonclustered index ix_index_column_partitioned_1 on index_column_partitioned_1 (test_column_2, test_column_1)"
        );
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop table index_column_columnstore_1",
        "drop table index_column_columnstore_2",
        "drop table if exists index_column_columnstore_3",
        "drop table index_column_partitioned_1",
        "drop partition scheme ps_index_column_1",
        "drop partition function pf_index_column_1"
    );

    [Test]
    public async Task Indexes_WhenGivenNonClusteredColumnStoreIndex_ReturnsIndexWithDeclaredColumns()
    {
        var table = await TableProvider.GetTable("index_column_columnstore_1").UnwrapSomeAsync();
        var index = table.Indexes.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Name.LocalName, Is.EqualTo("ix_index_column_columnstore_1"));
            Assert.That(index.IndexType, Is.EqualTo(IndexType.ColumnStore));
            Assert.That(GetColumnNames(index), Is.EqualTo(new[] { "test_column_3", "test_column_1" }));
            Assert.That(index.IncludedColumns, Is.Empty);
        }
    }

    [Test]
    public async Task Indexes_WhenGivenClusteredColumnStoreIndex_ReturnsIndexWithEveryColumn()
    {
        var table = await TableProvider.GetTable("index_column_columnstore_2").UnwrapSomeAsync();
        var index = table.Indexes.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.Name.LocalName, Is.EqualTo("ix_index_column_columnstore_2"));
            Assert.That(index.IndexType, Is.EqualTo(IndexType.ColumnStore));
            Assert.That(GetColumnNames(index), Is.EqualTo(new[] { "test_column_1", "test_column_2" }));
            Assert.That(index.IncludedColumns, Is.Empty);
        }
    }

    [Test]
    public async Task Indexes_WhenGivenOrderedClusteredColumnStoreIndex_ReturnsSortColumnsFirstInSortOrder()
    {
        if (!DatabaseProvider.SupportsOrderedColumnStoreIndex())
            Assert.Ignore("Ordered clustered columnstore indexes require SQL Server 2022 (16.x) or later.");

        var table = await TableProvider.GetTable("index_column_columnstore_3").UnwrapSomeAsync();
        var index = table.Indexes.Single();

        Assert.That(GetColumnNames(index), Is.EqualTo(new[] { "test_column_3", "test_column_2", "test_column_1" }));
    }

    [Test]
    public async Task GetAllTables_WhenColumnStoreIndexesPresent_ReturnsTablesWithColumnStoreIndexes()
    {
        var tables = await TableProvider.GetAllTables();
        var indexes = tables
            .Where(static t => t.Name.LocalName is "index_column_columnstore_1" or "index_column_columnstore_2")
            .SelectMany(static t => t.Indexes)
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexes, Has.Count.EqualTo(2));
            Assert.That(indexes.Select(static i => i.IndexType), Is.All.EqualTo(IndexType.ColumnStore));
        }
    }

    [Test]
    public async Task EnumerateAllTables_WhenColumnStoreIndexesPresent_ReturnsTablesWithColumnStoreIndexes()
    {
        var tables = await TableProvider.EnumerateAllTables()
            .Where(static t => t.Name.LocalName is "index_column_columnstore_1" or "index_column_columnstore_2")
            .ToListAsync();
        var indexes = tables.SelectMany(static t => t.Indexes).ToList();

        Assert.That(indexes, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Indexes_WhenGivenAlignedIndexWithoutPartitioningColumn_ReturnsOnlyDeclaredKeyColumns()
    {
        var table = await TableProvider.GetTable("index_column_partitioned_1").UnwrapSomeAsync();
        var index = table.Indexes.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetColumnNames(index), Is.EqualTo(new[] { "test_column_2", "test_column_1" }));
            Assert.That(index.IncludedColumns, Is.Empty);
        }
    }

    private static string[] GetColumnNames(IDatabaseIndex index) => index.Columns
        .SelectMany(static c => c.DependentColumns)
        .Select(static c => c.Name.LocalName)
        .ToArray();
}
