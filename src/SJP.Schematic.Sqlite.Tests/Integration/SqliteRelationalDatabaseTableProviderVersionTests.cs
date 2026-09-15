using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Which pragmas a provider uses is decided once from the SQLite version, and every later table load
// depends on that decision. A failure while reading the version must not stick.
internal sealed class SqliteRelationalDatabaseTableProviderVersionTests : SqliteTest
{
    private CachingConnectionFactory _connectionFactory;

    [OneTimeSetUp]
    public async Task Init()
    {
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=VersionLoad_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));
        await _connectionFactory.ExecuteAsync("create table version_load_table ( id integer primary key, value text )", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    [Test]
    public async Task LoadColumnsAsync_AfterVersionLoadFailed_LoadsColumnsOnceConnectionsSucceed()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var connectionFactory = new SwitchableFailureConnectionFactory(_connectionFactory) { IsFailing = true };
        var connection = new SchematicConnection(connectionFactory, Dialect);
        var tableProvider = new ColumnsTableProvider(connection, new ConnectionPragma(connection), new IdentifierDefaults(null, "main", "main"));
        var tableName = Identifier.CreateQualifiedIdentifier("main", "version_load_table");

        Assert.That(() => tableProvider.GetColumnsAsync(tableName, cancellationToken), Throws.TypeOf<SimulatedConnectionFailureException>());

        connectionFactory.IsFailing = false;
        var columns = await tableProvider.GetColumnsAsync(tableName, cancellationToken);

        Assert.That(columns, Has.Exactly(2).Items);
    }

    private sealed class ColumnsTableProvider : SqliteRelationalDatabaseTableProvider
    {
        public ColumnsTableProvider(ISchematicConnection connection, ISqliteConnectionPragma pragma, IIdentifierDefaults identifierDefaults)
            : base(connection, pragma, identifierDefaults)
        {
        }

        public Task<IReadOnlyList<IDatabaseColumn>> GetColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
            => LoadColumnsAsync(tableName, CreateQueryCache(cancellationToken), cancellationToken);
    }
}
