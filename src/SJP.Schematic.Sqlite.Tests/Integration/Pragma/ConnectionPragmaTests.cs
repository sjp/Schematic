using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration.Pragma;

internal sealed class ConnectionPragmaTests : SqliteTest
{
    private static IDbConnectionFactory CreateConnectionFactory()
    {
        var id = Guid.NewGuid().ToString().Replace('-', '_');
        var connectionString = $"Data Source=InMemory_{id};Mode=Memory;Cache=Shared";

        return new CachingConnectionFactory(new SqliteConnectionFactory(connectionString));
    }

    private static ISqliteConnectionPragma CreateConnectionPragma(IDbConnectionFactory connectionFactory)
    {
        var conn = new SchematicConnection(Guid.NewGuid(), connectionFactory, new SqliteDialect());
        return new ConnectionPragma(conn);
    }

    [Test]
    public static async Task DatabasePragmasAsync_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
    {
        var expectedSchemas = new HashSet<string>(["main", "temp"], StringComparer.OrdinalIgnoreCase);

        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var dbPragmas = await connPragma.DatabasePragmasAsync();
        var dbPragmaNames = dbPragmas.Select(d => d.SchemaName).ToList();
        var onlyExpectedPresent = dbPragmaNames.TrueForAll(expectedSchemas.Contains);

        Assert.That(onlyExpectedPresent, Is.True);
    }

    [Test]
    public static async Task DatabaseListAsync_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
    {
        var expectedSchemas = new HashSet<string>(["main", "temp"], StringComparer.OrdinalIgnoreCase);

        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var dbLists = await connPragma.DatabaseListAsync();
        var dbNames = dbLists.Select(d => d.name).ToList();
        var onlyExpectedPresent = dbNames.TrueForAll(expectedSchemas.Contains);

        Assert.That(onlyExpectedPresent, Is.True);
    }

    [Test]
    public static async Task AnalysisLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var analysisLimit = await connPragma.AnalysisLimitAsync();
        var newValue = analysisLimit + 100;
        await connPragma.AnalysisLimitAsync(newValue);
        var readOfNewValue = await connPragma.AnalysisLimitAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task AutomaticIndexAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var automaticIndex = await connPragma.AutomaticIndexAsync();
        var newValue = !automaticIndex;
        await connPragma.AutomaticIndexAsync(newValue);
        var readOfNewValue = await connPragma.AutomaticIndexAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task BusyTimeoutAsync_Get_ReadsCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var busyTimeout = await connPragma.BusyTimeoutAsync();
        var defaultValue = new TimeSpan(0, 0, 0);

        Assert.That(busyTimeout, Is.EqualTo(defaultValue));
    }

    [Test]
    [Ignore("SqliteConnection does not support setting of busy_timeout pragmas yet.")]
    public static async Task BusyTimeoutAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        _ = await connPragma.BusyTimeoutAsync();
        var newValue = new TimeSpan(0, 0, 23);
        await connPragma.BusyTimeoutAsync(newValue);
        var readOfNewValue = await connPragma.BusyTimeoutAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CaseSensitiveLikeAsync_WhenSet_WritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test ( col text )", CancellationToken.None);
        await connection.ExecuteAsync("insert into test (col) values ('dummy')", CancellationToken.None);
        await connection.ExecuteAsync("insert into test (col) values ('DUMMY_DUMMY')", CancellationToken.None);
        await connection.ExecuteAsync("insert into test (col) values ('DUMMY')", CancellationToken.None);

        const string query = "select count(*) from test where col like 'DUMMY%'";
        const int expectedInsensitive = 3;
        const int expectedSensitive = 2;

        await connPragma.CaseSensitiveLikeAsync(false);
        var insensitiveResult = await connection.ExecuteScalarAsync<int>(query, CancellationToken.None);

        await connPragma.CaseSensitiveLikeAsync(true);
        var sensitiveResult = await connection.ExecuteScalarAsync<int>(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(insensitiveResult, Is.EqualTo(expectedInsensitive));
            Assert.That(sensitiveResult, Is.EqualTo(expectedSensitive));
        }
    }

    [Test]
    public static async Task CellSizeCheckAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var cellSizeCheck = await connPragma.CellSizeCheckAsync();
        var newValue = !cellSizeCheck;
        await connPragma.CellSizeCheckAsync(newValue);
        var readOfNewValue = await connPragma.CellSizeCheckAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CheckpointFullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var checkpointFullFsync = await connPragma.CheckpointFullFsyncAsync();
        var newValue = !checkpointFullFsync;
        await connPragma.CheckpointFullFsyncAsync(newValue);
        var readOfNewValue = await connPragma.CheckpointFullFsyncAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CollationListAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var collations = await connPragma.CollationListAsync();

        Assert.That(collations, Is.Not.Empty);
    }

    [Test]
    public static async Task CompileOptionsAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var options = await connPragma.CompileOptionsAsync();

        Assert.That(options, Is.Not.Empty);
    }

    [Test]
    public static async Task DataVersionAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var dataVersion = await connPragma.DataVersionAsync();

        Assert.That(dataVersion, Is.Not.Zero);
    }

    [Test]
    public static async Task DeferForeignKeysAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var deferForeignKeys = await connPragma.DeferForeignKeysAsync();
        var newValue = !deferForeignKeys;
        await connPragma.DeferForeignKeysAsync(newValue);
        var readOfNewValue = await connPragma.DeferForeignKeysAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task EncodingAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var textEncoding = await connPragma.EncodingAsync();
        var newValue = textEncoding == Encoding.Utf8 ? Encoding.Utf16le : Encoding.Utf8;
        await connPragma.EncodingAsync(newValue);
        var readOfNewValue = await connPragma.EncodingAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void EncodingAsync_GivenInvalidEncodingValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        const Encoding newValue = (Encoding)55;
        Assert.That(() => connPragma.EncodingAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task ForeignKeysAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var foreignKeys = await connPragma.ForeignKeysAsync();
        var newValue = !foreignKeys;
        await connPragma.ForeignKeysAsync(newValue);
        var readOfNewValue = await connPragma.ForeignKeysAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task FullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var fullFsync = await connPragma.FullFsyncAsync();
        var newValue = !fullFsync;
        await connPragma.FullFsyncAsync(newValue);
        var readOfNewValue = await connPragma.FullFsyncAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void FunctionListAsync_WhenInvoked_ThrowsNothing()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.FunctionListAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task IgnoreCheckConstraintsAsync_WhenSet_WritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test ( col text, constraint col_ck check (col <> 'test') )", CancellationToken.None);

        await connPragma.IgnoreCheckConstraintsAsync(true);
        await connection.ExecuteAsync("insert into test (col) values ('test')", CancellationToken.None);

        await connPragma.IgnoreCheckConstraintsAsync(false);
        Assert.That(async () => await connection.ExecuteAsync("insert into test (col) values ('test')", CancellationToken.None), Throws.TypeOf<SqliteException>());
    }

    [Test]
    public static async Task LegacyAlterTableAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var legacyAlterTable = await connPragma.LegacyAlterTableAsync();
        var newValue = !legacyAlterTable;
        await connPragma.LegacyAlterTableAsync(newValue);
        var readOfNewValue = await connPragma.LegacyAlterTableAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void ModuleListAsync_WhenInvoked_ThrowsNothing()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.ModuleListAsync(), Throws.Nothing);
    }

    [Test]
    public static void OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.OptimizeAsync(), Throws.Nothing);
    }

    [Test]
    public static void OptimizeAsync_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        const OptimizeFeatures newValue = (OptimizeFeatures)55;
        Assert.That(() => connPragma.OptimizeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static void PragmaListAsync_WhenInvoked_ThrowsNothing()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.PragmaListAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task QueryOnlyAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var queryOnly = await connPragma.QueryOnlyAsync();
        var newValue = !queryOnly;
        await connPragma.QueryOnlyAsync(newValue);
        var readOfNewValue = await connPragma.QueryOnlyAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task ReadUncommittedAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var readUncommitted = await connPragma.ReadUncommittedAsync();
        var newValue = !readUncommitted;
        await connPragma.ReadUncommittedAsync(newValue);
        var readOfNewValue = await connPragma.ReadUncommittedAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task RecursiveTriggersAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var recursiveTriggers = await connPragma.RecursiveTriggersAsync();
        var newValue = !recursiveTriggers;
        await connPragma.RecursiveTriggersAsync(newValue);
        var readOfNewValue = await connPragma.RecursiveTriggersAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task ReverseUnorderedSelectsAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var recursiveTriggers = await connPragma.ReverseUnorderedSelectsAsync();
        var newValue = !recursiveTriggers;
        await connPragma.ReverseUnorderedSelectsAsync(newValue);
        var readOfNewValue = await connPragma.ReverseUnorderedSelectsAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void ShrinkMemoryAsync_WhenInvoked_PerformsOperationSuccessfully()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.ShrinkMemoryAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task SoftHeapLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var softHeapLimit = await connPragma.SoftHeapLimitAsync();
        var newValue = softHeapLimit == 0 ? 9000 : 0;
        await connPragma.SoftHeapLimitAsync(newValue);
        var readOfNewValue = await connPragma.SoftHeapLimitAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task TableListAsync_WhenTableExists_ReadsValuesCorrectlyForDatabase()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var tableInfo = await connPragma.TableListAsync();

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static async Task TemporaryStoreAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var tempStore = await connPragma.TemporaryStoreAsync();
        var newValue = tempStore == TemporaryStoreLocation.Default ? TemporaryStoreLocation.File : TemporaryStoreLocation.Default;
        await connPragma.TemporaryStoreAsync(newValue);
        var readOfNewValue = await connPragma.TemporaryStoreAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void TemporaryStoreAsync_GivenInvalidTemporaryStoreLocationValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        const TemporaryStoreLocation tempStore = (TemporaryStoreLocation)55;
        Assert.That(() => connPragma.TemporaryStoreAsync(tempStore), Throws.ArgumentException);
    }

    [Test]
    public static async Task ThreadsAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var threads = await connPragma.ThreadsAsync();
        var newValue = threads == 0 ? 8 : 0;
        await connPragma.ThreadsAsync(newValue);
        var readOfNewValue = await connPragma.ThreadsAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task WalAutoCheckpointAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var autoCheckpoint = await connPragma.WalAutoCheckpointAsync();
        var newValue = autoCheckpoint == 50 ? 100 : 50;
        await connPragma.WalAutoCheckpointAsync(newValue);
        var readOfNewValue = await connPragma.WalAutoCheckpointAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task WritableSchemaAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var writableSchema = await connPragma.WritableSchemaAsync();
        var newValue = !writableSchema;
        await connPragma.WritableSchemaAsync(newValue);
        var readOfNewValue = await connPragma.WritableSchemaAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }
}