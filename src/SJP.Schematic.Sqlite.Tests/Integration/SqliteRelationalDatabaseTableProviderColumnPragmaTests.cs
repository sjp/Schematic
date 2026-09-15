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
internal sealed class SqliteRelationalDatabaseTableProviderColumnPragmaTests : SqliteTest
{
    private CachingConnectionFactory _connectionFactory;
    private readonly ConcurrentQueue<string> _statements = new();

    [OneTimeSetUp]
    public async Task Init()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=ColumnPragmas_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));

        await _connectionFactory.ExecuteAsync(@"
create table pragma_parent_table (
    first_key text not null,
    second_key integer not null,
    doubled_key integer generated always as (second_key * 2) virtual,
    constraint pk_pragma_parent_table primary key (second_key, first_key)
) without rowid", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table pragma_child_table (
    id integer primary key,
    parent_first_key text,
    parent_second_key integer,
    constraint fk_pragma_child_table_parent foreign key (parent_second_key, parent_first_key) references pragma_parent_table (second_key, first_key)
)", cancellationToken);

        var connection = (SqliteConnection)await _connectionFactory.OpenConnectionAsync(cancellationToken);
        raw.sqlite3_trace(connection.Handle, (_, statement) => _statements.Enqueue(statement), null);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    private IRelationalDatabaseTableProvider CreateTableProvider()
    {
        var connection = new SchematicConnection(_connectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        return new SqliteRelationalDatabaseTableProvider(connection, new ConnectionPragma(connection), identifierDefaults);
    }

    [Test]
    public async Task GetTable_WhenTableHasPrimaryKeyAndForeignKey_ReadsColumnPragmaOncePerTableWithoutTableInfo()
    {
        var tableProvider = CreateTableProvider();
        _statements.Clear();

        var table = await tableProvider.GetTable("pragma_child_table", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var statements = _statements.ToList();

        var tableXInfoStatements = statements.Where(static s => s.Contains("table_xinfo", StringComparison.OrdinalIgnoreCase)).ToList();
        var tableInfoStatements = statements.Where(static s => s.Contains("table_info", StringComparison.OrdinalIgnoreCase)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.PrimaryKey.UnwrapSome().Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "id" }));
            Assert.That(table.ParentKeys, Has.Count.EqualTo(1));
            Assert.That(tableXInfoStatements, Has.Count.EqualTo(2));
            Assert.That(tableXInfoStatements, Has.One.Contains("pragma_child_table"));
            Assert.That(tableXInfoStatements, Has.One.Contains("pragma_parent_table"));
            Assert.That(tableInfoStatements, Is.Empty);
        }
    }

    [Test]
    public async Task GetTable_WhenPrimaryKeyOrderDiffersFromColumnOrderAndTableHasGeneratedColumn_ReturnsKeyColumnsInKeyOrder()
    {
        var tableProvider = CreateTableProvider();

        var table = await tableProvider.GetTable("pragma_parent_table", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var primaryKey = table.PrimaryKey.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "first_key", "second_key", "doubled_key" }));
            Assert.That(primaryKey.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "second_key", "first_key" }));
        }
    }
}
