using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    // Pins the round-trip count for a single GetTable() call, guarding against the per-table query count
    // creeping back up. table_test_table_2 is a single-column, primary-keyed table with no unique keys,
    // foreign keys, or incoming child keys of its own, so it isolates the query shape without dragging in
    // the child-key fan-out to other tables.
    //
    // A table load issues 7 queries: one to resolve the table's name, then columns, triggers, indexes,
    // child keys and storage options, plus a single constraints query that reads the primary key, unique
    // keys, foreign keys and check constraints together.
    [Test]
    public async Task GetTable_ForSingleTableWithOnlyAPrimaryKey_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var tableProvider = new OracleRelationalDatabaseTableProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await tableProvider.GetTable("table_test_table_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(7));
    }
}
