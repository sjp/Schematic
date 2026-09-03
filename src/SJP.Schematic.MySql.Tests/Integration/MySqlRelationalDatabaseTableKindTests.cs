using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed class MySqlRelationalDatabaseTableKindTests : MySqlTest
{
    private IRelationalDatabaseTableProvider TableProvider => new MySqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_kind_regular_1 ( test_column int ) collate utf8mb4_general_ci", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
create table table_kind_partitioned_1 (
    part_key int not null,
    payload varchar(50),
    primary key (part_key)
)
partition by range columns (part_key) (
    partition p0 values less than (100),
    partition p1 values less than (maxvalue)
)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public Task CleanUp() => DropTablesAsync(
        "table_kind_regular_1",
        "table_kind_partitioned_1"
    );

    [Test]
    public async Task GetTable_GivenOrdinaryTable_ReturnsLoggedRegularTableWithCollation()
    {
        var table = await TableProvider.GetTable("table_kind_regular_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(table.IsLogged, Is.True);
            Assert.That(table.Partitioning, OptionIs.None);
            Assert.That(table.SystemVersioning, OptionIs.None);
            Assert.That(table.Collation.UnwrapSome().LocalName, Is.EqualTo("utf8mb4_general_ci"));
        }
    }

    [Test]
    public async Task GetTable_GivenPartitionedTable_ReturnsPartitioningWithItsKeyColumnsAndPartitions()
    {
        var table = await TableProvider.GetTable("table_kind_partitioned_1").UnwrapSomeAsync();

        var partitioning = table.Partitioning.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.PartitionParent));
            Assert.That(partitioning.Strategy, Is.EqualTo("RANGE COLUMNS"));
            Assert.That(partitioning.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "part_key" }));
            Assert.That(partitioning.Partitions.Select(static p => p.LocalName), Is.EqualTo(new[] { "p0", "p1" }));
        }
    }
}
