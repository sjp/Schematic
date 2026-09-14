using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Runs against a private in-memory database, so the number of tables is known exactly and no other
// fixture's tables take part in the child key scan.
internal sealed class SqliteRelationalDatabaseTableProviderChildKeyQueryTests : SqliteTest
{
    private SqliteConnectionFactory _connectionFactory;
    private DbConnection _keepAliveConnection;

    [OneTimeSetUp]
    public async Task Init()
    {
        _connectionFactory = new SqliteConnectionFactory($"Data Source=ChildKeyQueries_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

        // a shared in-memory database only lives while a connection to it is open
        _keepAliveConnection = await _connectionFactory.OpenConnectionAsync(TestContext.CurrentContext.CancellationToken);

        await _connectionFactory.ExecuteAsync("create table parent_table ( id integer primary key, code text not null unique )", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table child_table (
    parent_id integer,
    parent_code text,
    constraint fk_child_table_parent_id foreign key (parent_id) references PARENT_TABLE (id),
    constraint fk_child_table_parent_code foreign key (parent_code) references Parent_Table (code)
)", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create table other_parent_table ( id integer primary key )", TestContext.CurrentContext.CancellationToken);
        await AddUnrelatedTablesAsync(0, 3);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_keepAliveConnection != null)
            await _keepAliveConnection.DisposeAsync();
    }

    private async Task AddUnrelatedTablesAsync(int start, int count)
    {
        for (var i = start; i < start + count; i++)
        {
            await _connectionFactory.ExecuteAsync(
                $"create table unrelated_table_{i} ( id integer primary key, other_parent_id integer references other_parent_table (id) )",
                TestContext.CurrentContext.CancellationToken);
        }
    }

    private (IRelationalDatabaseTableProvider TableProvider, CountingDbConnectionFactory ConnectionFactory) CreateCountingTableProvider()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(_connectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        var tableProvider = new SqliteRelationalDatabaseTableProvider(countingConnection, new ConnectionPragma(countingConnection), identifierDefaults);

        return (tableProvider, countingConnectionFactory);
    }

    [Test]
    public async Task GetTable_WhenChildReferencesParentNameInDifferentCase_ReturnsEachChildKeyOnce()
    {
        var (tableProvider, _) = CreateCountingTableProvider();

        var table = await tableProvider.GetTable("parent_table", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var childKeyNames = table.ChildKeys.Select(static k => k.ChildKey.Name.UnwrapSome().LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.ChildKeys.Select(static k => k.ChildTable.LocalName), Is.All.EqualTo("child_table"));
            Assert.That(childKeyNames, Is.EquivalentTo(new[] { "fk_child_table_parent_id", "fk_child_table_parent_code" }));
        }
    }

    // Tables that do not reference the requested table must only cost the foreign key list pragma used
    // to find child tables, not a load of their own foreign keys and the keys those refer to.
    [Test]
    public async Task GetTable_WhenUnrelatedTablesAreAdded_IssuesOneAdditionalQueryPerUnrelatedTable()
    {
        const int addedTableCount = 4;

        var (tableProvider, countingConnectionFactory) = CreateCountingTableProvider();
        _ = await tableProvider.GetTable("parent_table", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var queryCountBefore = countingConnectionFactory.QueryCount;

        await AddUnrelatedTablesAsync(1000, addedTableCount);

        var (tableProviderAfter, countingConnectionFactoryAfter) = CreateCountingTableProvider();
        var table = await tableProviderAfter.GetTable("parent_table", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var queryCountAfter = countingConnectionFactoryAfter.QueryCount;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.ChildKeys, Has.Count.EqualTo(2));
            Assert.That(queryCountAfter - queryCountBefore, Is.EqualTo(addedTableCount));
        }
    }
}
