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

// One database object is reused across ATTACH, DETACH and DDL, so anything it remembers from an
// earlier call must not hide what has changed since. The caching connection factory keeps a single
// connection open, which ATTACH needs in order to persist between commands.
internal sealed class SqliteRelationalDatabaseStaleMetadataTests : SqliteTest
{
    private TemporaryDirectory _tempDirectory;
    private string _attachedDatabasePath;
    private CachingConnectionFactory _connectionFactory;
    private SqliteRelationalDatabase _database;

    private const string AttachedSchemaName = "other";

    [SetUp]
    public async Task SetUp()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        _tempDirectory = new TemporaryDirectory();
        var mainDatabasePath = Path.Combine(_tempDirectory.DirectoryPath, "main.sqlite");
        _attachedDatabasePath = Path.Combine(_tempDirectory.DirectoryPath, "other.sqlite");

        var attachedFactory = new SqliteConnectionFactory($"Data Source={_attachedDatabasePath}");
        await attachedFactory.ExecuteAsync("create table other_table ( id integer primary key )", cancellationToken);
        await attachedFactory.ExecuteAsync("create view other_view as select id from other_table", cancellationToken);

        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source={mainDatabasePath}"));
        await _connectionFactory.ExecuteAsync("create table main_table ( id integer primary key )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view main_view as select id from main_table", cancellationToken);

        var connection = new SchematicConnection(_connectionFactory, Dialect);
        _database = new SqliteRelationalDatabase(connection, new IdentifierDefaults(null, "main", "main"), new ConnectionPragma(connection));
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
    public async Task GetAllTables_WhenDatabaseAttachedAfterTablesLoaded_IncludesAttachedTables()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllTables(cancellationToken);
        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        var tables = await _database.GetAllTables(cancellationToken);

        Assert.That(tables.Select(static t => (t.Name.Schema, t.Name.LocalName)), Is.EqualTo(new[] { ("main", "main_table"), (AttachedSchemaName, "other_table") }));
    }

    [Test]
    public async Task EnumerateAllTables_WhenDatabaseAttachedAfterTablesLoaded_IncludesAttachedTables()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.EnumerateAllTables(cancellationToken).ToListAsync(cancellationToken);
        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        var tables = await _database.EnumerateAllTables(cancellationToken).ToListAsync(cancellationToken);

        Assert.That(tables.Select(static t => (t.Name.Schema, t.Name.LocalName)), Is.EqualTo(new[] { ("main", "main_table"), (AttachedSchemaName, "other_table") }));
    }

    [Test]
    public async Task GetTable_WhenGivenUnqualifiedNameInDatabaseAttachedAfterTablesLoaded_ReturnsTable()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllTables(cancellationToken);
        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        var table = await _database.GetTable(Identifier.CreateQualifiedIdentifier("other_table"), cancellationToken).UnwrapSomeAsync();

        Assert.That(table.Name.Schema, Is.EqualTo(AttachedSchemaName));
    }

    [Test]
    public async Task GetAllViews_WhenDatabaseAttachedAfterViewsLoaded_IncludesAttachedViews()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllViews(cancellationToken);
        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        var views = await _database.GetAllViews(cancellationToken);

        Assert.That(views.Select(static v => (v.Name.Schema, v.Name.LocalName)), Is.EqualTo(new[] { ("main", "main_view"), (AttachedSchemaName, "other_view") }));
    }

    [Test]
    public async Task GetView_WhenGivenUnqualifiedNameInDatabaseAttachedAfterViewsLoaded_ReturnsView()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllViews(cancellationToken);
        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        var view = await _database.GetView(Identifier.CreateQualifiedIdentifier("other_view"), cancellationToken).UnwrapSomeAsync();

        Assert.That(view.Name.Schema, Is.EqualTo(AttachedSchemaName));
    }

    [Test]
    public async Task GetAllTables_WhenDatabaseDetachedAfterTablesLoaded_OmitsDetachedTables()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        await _database.GetAllTables(cancellationToken);
        await _database.DetachDatabaseAsync(AttachedSchemaName, cancellationToken);
        var tables = await _database.GetAllTables(cancellationToken);

        Assert.That(tables.Select(static t => (t.Name.Schema, t.Name.LocalName)), Is.EqualTo(new[] { ("main", "main_table") }));
    }

    [Test]
    public async Task GetAllViews_WhenDatabaseDetachedAfterViewsLoaded_OmitsDetachedViews()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.AttachDatabaseAsync(AttachedSchemaName, _attachedDatabasePath, cancellationToken);
        await _database.GetAllViews(cancellationToken);
        await _database.DetachDatabaseAsync(AttachedSchemaName, cancellationToken);
        var views = await _database.GetAllViews(cancellationToken);

        Assert.That(views.Select(static v => (v.Name.Schema, v.Name.LocalName)), Is.EqualTo(new[] { ("main", "main_view") }));
    }

    [Test]
    public async Task GetTable_WhenVirtualTableCreatedAfterTablesLoaded_ReturnsVirtualKind()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllTables(cancellationToken);
        await _connectionFactory.ExecuteAsync("create virtual table late_fts using fts5 ( body )", cancellationToken);
        var table = await _database.GetTable(Identifier.CreateQualifiedIdentifier("main", "late_fts"), cancellationToken).UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.Virtual));
    }

    [Test]
    public async Task GetAllTables_WhenVirtualTableCreatedAfterTablesLoaded_OmitsShadowTables()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllTables(cancellationToken);
        await _connectionFactory.ExecuteAsync("create virtual table late_fts using fts5 ( body )", cancellationToken);
        var tables = await _database.GetAllTables(cancellationToken);

        Assert.That(tables.Select(static t => t.Name.LocalName), Is.EqualTo(new[] { "late_fts", "main_table" }));
    }

    [Test]
    public async Task GetTable_WhenWithoutRowIdTableCreatedAfterTablesLoaded_ReturnsIndexOrganizedKind()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await _database.GetAllTables(cancellationToken);
        await _connectionFactory.ExecuteAsync("create table late_without_rowid ( id integer not null primary key ) without rowid", cancellationToken);
        var table = await _database.GetTable(Identifier.CreateQualifiedIdentifier("main", "late_without_rowid"), cancellationToken).UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.IndexOrganized));
    }
}
