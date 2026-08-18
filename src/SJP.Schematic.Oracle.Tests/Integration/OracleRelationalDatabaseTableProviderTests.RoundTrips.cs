using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    // Pins the round-trip count for a single GetTable() call, guarding against the per-table query count
    // creeping back up. table_test_table_2 is a single-column, primary-keyed table with no unique keys,
    // foreign keys, or incoming child keys of its own, so it isolates the query shape from the
    // constraint-merge fix without dragging in the child-key fan-out to other tables.
    //
    // Before the GetTablePrimaryKey/GetTableUniqueKeys/GetTableParentKeys -> GetTableConstraints merge,
    // a table load issued 8 distinct queries (columns, checks, triggers, indexes, primary key, unique
    // keys, parent keys, child keys); it now issues 6 (constraints merged into one). Plus one query to
    // resolve the table's name up front, for 7 total.
    [Test]
    public async Task GetTable_ForSingleTableWithOnlyAPrimaryKey_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var tableProvider = new OracleRelationalDatabaseTableProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await tableProvider.GetTable("table_test_table_2", CancellationToken.None).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(7));
    }
}
