using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// A provider lives as long as the database object that created it, so what it remembers about a
// parsed CREATE statement must follow the object it belongs to: the latest definition replaces the
// previous one instead of accumulating alongside it.
internal sealed class SqliteParsedDefinitionCacheTests : SqliteTest
{
    private TemporaryDirectory _tempDirectory;
    private CachingConnectionFactory _connectionFactory;
    private SqliteRelationalDatabaseTableProvider _tableProvider;
    private SqliteDatabaseViewProvider _viewProvider;

    [SetUp]
    public async Task SetUp()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        _tempDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(_tempDirectory.DirectoryPath, "main.sqlite");

        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source={databasePath}"));
        await _connectionFactory.ExecuteAsync("create table cache_test_table ( id integer primary key )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table cache_test_audit ( id integer )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger cache_test_trigger after insert on cache_test_table begin insert into cache_test_audit ( id ) values ( new.id ); end", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view cache_test_view as select id from cache_test_table", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger cache_test_view_trigger instead of insert on cache_test_view begin insert into cache_test_table ( id ) values ( new.id ); end", cancellationToken);

        var connection = new SchematicConnection(_connectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        var pragma = new ConnectionPragma(connection);

        _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, identifierDefaults);
        _viewProvider = new SqliteDatabaseViewProvider(connection, pragma, identifierDefaults);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();

        // pooled connections keep the database files open
        SqliteConnection.ClearAllPools();
        _tempDirectory?.Dispose();
    }

    [Test]
    public async Task GetTable_WhenColumnAddedAfterTableLoaded_ReturnsAddedColumnAndKeepsOneCachedDefinition()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var tableName = Identifier.CreateQualifiedIdentifier("main", "cache_test_table");

        await _tableProvider.GetTable(tableName, cancellationToken).UnwrapSomeAsync();
        await _connectionFactory.ExecuteAsync("alter table cache_test_table add column added_column integer", cancellationToken);
        var table = await _tableProvider.GetTable(tableName, cancellationToken).UnwrapSomeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(table.Columns.Select(static c => c.Name.LocalName), Does.Contain("added_column"));
            Assert.That(_tableProvider.ParsedTableCacheCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetTable_WhenTriggerRedefinedAfterTableLoaded_ReturnsNewTriggerAndKeepsOneCachedDefinition()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var tableName = Identifier.CreateQualifiedIdentifier("main", "cache_test_table");

        await _tableProvider.GetTable(tableName, cancellationToken).UnwrapSomeAsync();
        await _connectionFactory.ExecuteAsync("drop trigger cache_test_trigger", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger cache_test_trigger after delete on cache_test_table begin insert into cache_test_audit ( id ) values ( old.id ); end", cancellationToken);
        var table = await _tableProvider.GetTable(tableName, cancellationToken).UnwrapSomeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(table.Triggers.Single().TriggerEvent, Is.EqualTo(TriggerEvent.Delete));
            Assert.That(_tableProvider.ParsedTriggerCacheCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetView_WhenTriggerRedefinedAfterViewLoaded_ReturnsNewTriggerAndKeepsOneCachedDefinition()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var viewName = Identifier.CreateQualifiedIdentifier("main", "cache_test_view");

        await _viewProvider.GetView(viewName, cancellationToken).UnwrapSomeAsync();
        await _connectionFactory.ExecuteAsync("drop trigger cache_test_view_trigger", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger cache_test_view_trigger instead of delete on cache_test_view begin delete from cache_test_table where id = old.id; end", cancellationToken);
        var view = await _viewProvider.GetView(viewName, cancellationToken).UnwrapSomeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(view.Triggers.Single().TriggerEvent, Is.EqualTo(TriggerEvent.Delete));
            Assert.That(_viewProvider.ParsedTriggerCacheCount, Is.EqualTo(1));
        });
    }
}
