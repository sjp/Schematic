using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed class OracleRelationalDatabaseTableKindTests : OracleTest
{
    private IRelationalDatabaseTableProvider TableProvider => new OracleRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_kind_regular_1 ( test_column number )", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
create table table_kind_iot_1 (
    id number not null,
    payload varchar2(50),
    constraint pk_table_kind_iot_1 primary key (id)
) organization index", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
create table table_kind_partitioned_1 (
    part_key number not null,
    payload varchar2(50)
)
partition by range (part_key) (
    partition p0 values less than (100),
    partition p1 values less than (maxvalue)
)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_kind_regular_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_kind_iot_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_kind_partitioned_1", CancellationToken.None);
    }

    [Test]
    public async Task GetTable_GivenOrdinaryTable_ReturnsLoggedRegularTable()
    {
        var table = await TableProvider.GetTable("table_kind_regular_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(table.IsLogged, Is.True);
            Assert.That(table.Partitioning, OptionIs.None);
            // Oracle has no system-versioned tables
            Assert.That(table.SystemVersioning, OptionIs.None);
        }
    }

    [Test]
    public async Task GetTable_GivenIndexOrganizedTable_ReturnsIndexOrganizedKind()
    {
        var table = await TableProvider.GetTable("table_kind_iot_1").UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.IndexOrganized));
    }

    [Test]
    public async Task GetTable_GivenPartitionedTable_ReturnsPartitioningWithItsKeyColumnsAndPartitions()
    {
        var table = await TableProvider.GetTable("table_kind_partitioned_1").UnwrapSomeAsync();

        var partitioning = table.Partitioning.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.PartitionParent));
            Assert.That(partitioning.Strategy, Is.EqualTo("RANGE"));
            Assert.That(partitioning.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "PART_KEY" }));
            Assert.That(partitioning.Partitions.Select(static p => p.LocalName), Is.EqualTo(new[] { "P0", "P1" }));
        }
    }
}
