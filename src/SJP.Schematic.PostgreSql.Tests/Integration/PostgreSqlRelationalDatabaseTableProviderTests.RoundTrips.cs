using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
{
    // Pins the round-trip count for a single GetTable() call on a table with one child table, where
    // that child table also references a third table. Building the child key must only read the child
    // table's name and columns; it must not load the child's other foreign keys, and so must not touch
    // the third table (its name, primary key, indexes or columns) at all.
    //
    // The parent table itself costs 9 queries: name resolution, then columns, checks, triggers,
    // indexes, primary and unique keys (read together), parent keys, child keys and table options. The
    // child key adds the child table's name and columns, for 11 in total.
    [Test]
    public async Task GetTable_ForTableWithChildTableReferencingAnotherTable_DoesNotLoadTheOtherTable()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var tableProvider = new PostgreSqlRelationalDatabaseTableProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        var table = await tableProvider.GetTable("child_key_round_trip_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.ChildKeys, Has.Exactly(1).Items);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(11));
        }
    }
}
