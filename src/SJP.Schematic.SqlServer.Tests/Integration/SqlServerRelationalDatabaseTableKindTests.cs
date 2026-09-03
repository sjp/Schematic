using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerRelationalDatabaseTableKindTests : SqlServerTest
{
    private IRelationalDatabaseTableProvider TableProvider => new SqlServerRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_kind_regular_1 ( test_column int )", CancellationToken.None);

        await DbConnection.ExecuteAsync(@"
create table table_kind_temporal_1 (
    id int not null primary key clustered,
    valid_from datetime2 generated always as row start not null,
    valid_to datetime2 generated always as row end not null,
    period for system_time (valid_from, valid_to)
) with (system_versioning = on (history_table = dbo.table_kind_temporal_1_history))", CancellationToken.None);

        await ExecuteBatchAsync(
            "create partition function pf_table_kind_1 (int) as range left for values (100)",
            "create partition scheme ps_table_kind_1 as partition pf_table_kind_1 all to ([PRIMARY])"
        );
        await DbConnection.ExecuteAsync(@"
create table table_kind_partitioned_1 (
    part_key int not null,
    payload varchar(50)
) on ps_table_kind_1 (part_key)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop table table_kind_regular_1",
        "alter table table_kind_temporal_1 set (system_versioning = off)",
        "drop table table_kind_temporal_1",
        "drop table table_kind_temporal_1_history",
        "drop table table_kind_partitioned_1",
        "drop partition scheme ps_table_kind_1",
        "drop partition function pf_table_kind_1"
    );

    [Test]
    public async Task GetTable_GivenOrdinaryTable_ReturnsLoggedRegularTable()
    {
        var table = await TableProvider.GetTable("table_kind_regular_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(table.IsLogged, Is.True);
            Assert.That(table.Partitioning, OptionIs.None);
            Assert.That(table.SystemVersioning, OptionIs.None);
            // SQL Server has no table-level collation; it is defined per character column
            Assert.That(table.Collation, OptionIs.None);
        }
    }

    [Test]
    public async Task GetTable_GivenSystemVersionedTable_ReturnsItsHistoryTableAndPeriodColumns()
    {
        var table = await TableProvider.GetTable("table_kind_temporal_1").UnwrapSomeAsync();

        var systemVersioning = table.SystemVersioning.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(systemVersioning.HistoryTable.LocalName, Is.EqualTo("table_kind_temporal_1_history"));
            Assert.That(systemVersioning.PeriodStartColumn.LocalName, Is.EqualTo("valid_from"));
            Assert.That(systemVersioning.PeriodEndColumn.LocalName, Is.EqualTo("valid_to"));
        }
    }

    [Test]
    public async Task GetTable_GivenHistoryTable_ReturnsHistoryKind()
    {
        var table = await TableProvider.GetTable("table_kind_temporal_1_history").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.History));
            Assert.That(table.SystemVersioning, OptionIs.None);
        }
    }

    [Test]
    public async Task GetTable_GivenPartitionedTable_ReturnsPartitioningOnItsSchemeAndKeyColumn()
    {
        var table = await TableProvider.GetTable("table_kind_partitioned_1").UnwrapSomeAsync();

        var partitioning = table.Partitioning.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.PartitionParent));
            Assert.That(partitioning.Strategy, Is.EqualTo("ps_table_kind_1"));
            Assert.That(partitioning.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "part_key" }));
            // SQL Server numbers partitions rather than naming them, so none are reported
            Assert.That(partitioning.Partitions, Is.Empty);
        }
    }
}
