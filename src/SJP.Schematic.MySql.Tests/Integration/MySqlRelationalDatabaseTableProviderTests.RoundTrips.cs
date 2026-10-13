using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
{
    // Pins the round-trip count for a single GetTable() call on a table with one child table, where
    // that child table also references a third table. Building the child key must only read the child
    // table's name, columns and foreign key rows. It must not load the child's other foreign keys, and
    // so must not touch the third table (its name, primary key, indexes or columns) at all.
    //
    // The parent table itself costs 7 queries: name resolution, then columns, catalog features, indexes
    // (from which the primary and unique keys are built), parent keys, child keys, and the table's options,
    // checks and triggers (read together). The child key adds the child table's name, columns and foreign
    // key rows, for 10 in total. The foreign
    // key rows are the same cached rows the child table's own parent keys are built from, so when every
    // table is loaded together a child key costs no query of its own.
    [Test]
    public async Task GetTable_ForTableWithChildTableReferencingAnotherTable_DoesNotLoadTheOtherTable()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var tableProvider = new MySqlRelationalDatabaseTableProvider(countingConnection, IdentifierDefaults);

        var table = await tableProvider.GetTable("child_key_round_trip_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.ChildKeys, Has.Exactly(1).Items);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(10));
        }
    }
}
