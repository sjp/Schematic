using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// A caching connection factory permits a single query at a time, so any provider that loads objects while
// its own name query is still being streamed would wait forever for the query slot that stream holds.
// Runs against a private in-memory database so the expected object names are known exactly.
[CancelAfter(30 * 1000)]
internal sealed class SqliteSingleConnectionEnumerationTests : SqliteTest
{
    private SqliteConnectionFactory _connectionFactory;
    private DbConnection _keepAliveConnection;
    private CachingConnectionFactory _cachingConnectionFactory;

    private static readonly string[] ExpectedTableNames = ["child_table", "parent_table", "unrelated_table"];
    private static readonly string[] ExpectedViewNames = ["child_view", "parent_view"];

    [OneTimeSetUp]
    public async Task Init()
    {
        _connectionFactory = new SqliteConnectionFactory($"Data Source=SingleConnectionEnumeration_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

        // a shared in-memory database only lives while a connection to it is open
        _keepAliveConnection = await _connectionFactory.OpenConnectionAsync(TestContext.CurrentContext.CancellationToken);

        await _connectionFactory.ExecuteAsync("create table parent_table ( id integer primary key, code text not null unique )", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create table child_table ( id integer primary key, parent_id integer references parent_table (id), note text )", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create index ix_child_table_note on child_table (note)", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger trg_child_table_insert after insert on child_table begin select 1; end", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create table unrelated_table ( id integer primary key )", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create view parent_view as select id, code from parent_table", TestContext.CurrentContext.CancellationToken);
        await _connectionFactory.ExecuteAsync("create view child_view as select id, parent_id from child_table", TestContext.CurrentContext.CancellationToken);

        _cachingConnectionFactory = new CachingConnectionFactory(_connectionFactory);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_cachingConnectionFactory != null)
            await _cachingConnectionFactory.DisposeAsync();

        if (_keepAliveConnection != null)
            await _keepAliveConnection.DisposeAsync();
    }

    private (SqliteRelationalDatabaseTableProvider TableProvider, SqliteDatabaseViewProvider ViewProvider) CreateProviders()
    {
        var connection = new SchematicConnection(_cachingConnectionFactory, Dialect);
        var pragma = new ConnectionPragma(connection);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");

        return (
            new SqliteRelationalDatabaseTableProvider(connection, pragma, identifierDefaults),
            new SqliteDatabaseViewProvider(connection, pragma, identifierDefaults)
        );
    }

    [Test]
    public async Task EnumerateAllTables_WhenOnlyOneQueryMayRunAtOnce_ReturnsAllTables()
    {
        var (tableProvider, _) = CreateProviders();

        var tables = await tableProvider.EnumerateAllTables(TestContext.CurrentContext.CancellationToken).ToListAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(tables.Select(static t => t.Name.LocalName), Is.EqualTo(ExpectedTableNames));
    }

    [Test]
    public async Task GetAllTables_WhenOnlyOneQueryMayRunAtOnce_ReturnsAllTables()
    {
        var (tableProvider, _) = CreateProviders();

        var tables = await tableProvider.GetAllTables(TestContext.CurrentContext.CancellationToken);

        Assert.That(tables.Select(static t => t.Name.LocalName), Is.EqualTo(ExpectedTableNames));
    }

    [Test]
    public async Task EnumerateAllViews_WhenOnlyOneQueryMayRunAtOnce_ReturnsAllViews()
    {
        var (_, viewProvider) = CreateProviders();

        var views = await viewProvider.EnumerateAllViews(TestContext.CurrentContext.CancellationToken).ToListAsync(TestContext.CurrentContext.CancellationToken);

        Assert.That(views.Select(static v => v.Name.LocalName), Is.EqualTo(ExpectedViewNames));
    }

    [Test]
    public async Task GetAllViews_WhenOnlyOneQueryMayRunAtOnce_ReturnsAllViews()
    {
        var (_, viewProvider) = CreateProviders();

        var views = await viewProvider.GetAllViews(TestContext.CurrentContext.CancellationToken);

        Assert.That(views.Select(static v => v.Name.LocalName), Is.EqualTo(ExpectedViewNames));
    }

    [Test]
    public async Task EnumerateAllTables_WhenConsumerQueriesWhileEnumerating_DoesNotWaitOnItself()
    {
        var (tableProvider, _) = CreateProviders();
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        var columnCounts = new List<long>();
        await foreach (var table in tableProvider.EnumerateAllTables(cancellationToken))
        {
            var sql = $"select count(*) from pragma_table_info('{table.Name.LocalName}')";
            columnCounts.Add(await _cachingConnectionFactory.ExecuteScalarAsync<long>(sql, cancellationToken));
        }

        Assert.That(columnCounts, Is.EqualTo(new long[] { 3, 2, 1 }));
    }
}
