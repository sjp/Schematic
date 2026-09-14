using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
{
    // Pins the round-trip count for a single GetTable() call, guarding against the per-table query count
    // creeping back up. table_test_table_2 is a single-column, primary-keyed table with no unique keys,
    // foreign keys or child keys, so no other table is loaded.
    //
    // The primary and unique keys are built from the index rows rather than queried separately, so a
    // table load issues 8 queries: name resolution, then columns, checks, triggers, indexes, parent keys,
    // child keys and table options.
    [Test]
    public async Task GetTable_ForSingleTableWithOnlyAPrimaryKey_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Connection.Dialect);
        var tableProvider = new SqlServerRelationalDatabaseTableProvider(countingConnection, IdentifierDefaults);

        var table = await tableProvider.GetTable("table_test_table_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.PrimaryKey, OptionIs.Some);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(8));
        }
    }
}
