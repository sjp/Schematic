using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;
using SQLitePCL;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Runs against a private in-memory database held by a single connection, so every statement the
// provider issues passes through one trace callback.
internal sealed class SqliteDatabaseViewProviderStatementTests : SqliteTest
{
    private CachingConnectionFactory _connectionFactory;
    private readonly ConcurrentQueue<string> _statements = new();

    [OneTimeSetUp]
    public async Task Init()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=ViewStatements_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));

        await _connectionFactory.ExecuteAsync("create table statement_view_table (id integer primary key, value integer)", cancellationToken);
        await _connectionFactory.ExecuteAsync("CREATE VIEW Statement_View as select id, value from statement_view_table", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger statement_view_insert instead of insert on statement_view begin select 1; end", cancellationToken);
        await _connectionFactory.ExecuteAsync("create trigger statement_view_delete instead of delete on STATEMENT_VIEW begin select 1; end", cancellationToken);

        var connection = (SqliteConnection)await _connectionFactory.OpenConnectionAsync(cancellationToken);
        raw.sqlite3_trace(connection.Handle, (_, statement) => _statements.Enqueue(statement), null);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    private IDatabaseViewProvider CreateViewProvider()
    {
        var connection = new SchematicConnection(_connectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        return new SqliteDatabaseViewProvider(connection, new ConnectionPragma(connection), identifierDefaults);
    }

    [Test]
    public async Task GetView_WhenViewHasTriggers_ReadsDefinitionAndTriggersInOneQuery()
    {
        var viewProvider = CreateViewProvider();
        _statements.Clear();

        var view = await viewProvider.GetView("statement_view", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var definitionStatements = _statements
            .Where(static s => s.Contains("sqlite_master", StringComparison.OrdinalIgnoreCase) && s.Contains("tbl_name", StringComparison.OrdinalIgnoreCase))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Definition, Is.EqualTo("CREATE VIEW Statement_View as select id, value from statement_view_table"));
            Assert.That(view.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "id", "value" }));
            Assert.That(view.Triggers.Select(static t => t.Name.LocalName), Is.EquivalentTo(new[] { "statement_view_insert", "statement_view_delete" }));
            Assert.That(view.IsUpdatable, Is.True);
            Assert.That(definitionStatements, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task GetAllViews_WhenViewHasTriggers_ReadsDefinitionAndTriggersInOneQueryPerView()
    {
        var viewProvider = CreateViewProvider();
        _statements.Clear();

        var views = await viewProvider.GetAllViews(TestContext.CurrentContext.CancellationToken);
        var definitionStatements = _statements
            .Where(static s => s.Contains("sqlite_master", StringComparison.OrdinalIgnoreCase) && s.Contains("tbl_name", StringComparison.OrdinalIgnoreCase))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(views, Has.Count.EqualTo(1));
            Assert.That(views.Single().Triggers, Has.Count.EqualTo(2));
            Assert.That(definitionStatements, Has.Count.EqualTo(1));
        }
    }
}
