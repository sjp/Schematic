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
        var id = Guid.NewGuid().ToString().Replace("-", "_", StringComparison.Ordinal);
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

        var dbPragmas = await connPragma.DatabasePragmasAsync().ConfigureAwait(false);
        var dbPragmaNames = dbPragmas.Select(d => d.SchemaName).ToList();
        var onlyExpectedPresent = dbPragmaNames.TrueForAll(name => expectedSchemas.Contains(name));

        Assert.That(onlyExpectedPresent, Is.True);
    }

    [Test]
    public static async Task DatabaseListAsync_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
    {
        var expectedSchemas = new HashSet<string>(["main", "temp"], StringComparer.OrdinalIgnoreCase);

        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var dbLists = await connPragma.DatabaseListAsync().ConfigureAwait(false);
        var dbNames = dbLists.Select(d => d.name).ToList();
        var onlyExpectedPresent = dbNames.TrueForAll(name => expectedSchemas.Contains(name));

        Assert.That(onlyExpectedPresent, Is.True);
    }

    [Test]
    public static async Task AnalysisLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var analysisLimit = await connPragma.AnalysisLimitAsync().ConfigureAwait(false);
        var newValue = analysisLimit + 100;
        await connPragma.AnalysisLimitAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.AnalysisLimitAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task AutomaticIndexAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var automaticIndex = await connPragma.AutomaticIndexAsync().ConfigureAwait(false);
        var newValue = !automaticIndex;
        await connPragma.AutomaticIndexAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.AutomaticIndexAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task BusyTimeoutAsync_Get_ReadsCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        var busyTimeout = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);
        var defaultValue = new TimeSpan(0, 0, 0);

        Assert.That(busyTimeout, Is.EqualTo(defaultValue));
    }

    [Test]
    [Ignore("SqliteConnection does not support setting of busy_timeout pragmas yet.")]
    public static async Task BusyTimeoutAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connectionFactory = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connectionFactory);

        _ = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);
        var newValue = new TimeSpan(0, 0, 23);
        await connPragma.BusyTimeoutAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CaseSensitiveLikeAsync_WhenSet_WritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test ( col text )", CancellationToken.None).ConfigureAwait(false);
        await connection.ExecuteAsync("insert into test (col) values ('dummy')", CancellationToken.None).ConfigureAwait(false);
        await connection.ExecuteAsync("insert into test (col) values ('DUMMY_DUMMY')", CancellationToken.None).ConfigureAwait(false);
        await connection.ExecuteAsync("insert into test (col) values ('DUMMY')", CancellationToken.None).ConfigureAwait(false);

        const string query = "select count(*) from test where col like 'DUMMY%'";
        const int expectedInsensitive = 3;
        const int expectedSensitive = 2;

        await connPragma.CaseSensitiveLikeAsync(false).ConfigureAwait(false);
        var insensitiveResult = await connection.ExecuteScalarAsync<int>(query, CancellationToken.None).ConfigureAwait(false);

        await connPragma.CaseSensitiveLikeAsync(true).ConfigureAwait(false);
        var sensitiveResult = await connection.ExecuteScalarAsync<int>(query, CancellationToken.None).ConfigureAwait(false);

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

        var cellSizeCheck = await connPragma.CellSizeCheckAsync().ConfigureAwait(false);
        var newValue = !cellSizeCheck;
        await connPragma.CellSizeCheckAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.CellSizeCheckAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CheckpointFullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var checkpointFullFsync = await connPragma.CheckpointFullFsyncAsync().ConfigureAwait(false);
        var newValue = !checkpointFullFsync;
        await connPragma.CheckpointFullFsyncAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.CheckpointFullFsyncAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CollationListAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var collations = await connPragma.CollationListAsync().ConfigureAwait(false);

        Assert.That(collations, Is.Not.Empty);
    }

    [Test]
    public static async Task CompileOptionsAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var options = await connPragma.CompileOptionsAsync().ConfigureAwait(false);

        Assert.That(options, Is.Not.Empty);
    }

    [Test]
    public static async Task DataVersionAsync_GetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var dataVersion = await connPragma.DataVersionAsync().ConfigureAwait(false);

        Assert.That(dataVersion, Is.Not.Zero);
    }

    [Test]
    public static async Task DeferForeignKeysAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var deferForeignKeys = await connPragma.DeferForeignKeysAsync().ConfigureAwait(false);
        var newValue = !deferForeignKeys;
        await connPragma.DeferForeignKeysAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.DeferForeignKeysAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task EncodingAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var textEncoding = await connPragma.EncodingAsync().ConfigureAwait(false);
        var newValue = textEncoding == Encoding.Utf8 ? Encoding.Utf16le : Encoding.Utf8;
        await connPragma.EncodingAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.EncodingAsync().ConfigureAwait(false);

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

        var foreignKeys = await connPragma.ForeignKeysAsync().ConfigureAwait(false);
        var newValue = !foreignKeys;
        await connPragma.ForeignKeysAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.ForeignKeysAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task FullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var fullFsync = await connPragma.FullFsyncAsync().ConfigureAwait(false);
        var newValue = !fullFsync;
        await connPragma.FullFsyncAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.FullFsyncAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void FunctionListAsync_WhenInvoked_ThrowsNothing()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.FunctionListAsync().ConfigureAwait(false), Throws.Nothing);
    }

    [Test]
    public static async Task IgnoreCheckConstraintsAsync_WhenSet_WritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test ( col text, constraint col_ck check (col <> 'test') )", CancellationToken.None).ConfigureAwait(false);

        await connPragma.IgnoreCheckConstraintsAsync(true).ConfigureAwait(false);
        await connection.ExecuteAsync("insert into test (col) values ('test')", CancellationToken.None).ConfigureAwait(false);

        await connPragma.IgnoreCheckConstraintsAsync(false).ConfigureAwait(false);
        Assert.That(async () => await connection.ExecuteAsync("insert into test (col) values ('test')", CancellationToken.None).ConfigureAwait(false), Throws.TypeOf<SqliteException>());
    }

    [Test]
    public static async Task LegacyAlterTableAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var legacyAlterTable = await connPragma.LegacyAlterTableAsync().ConfigureAwait(false);
        var newValue = !legacyAlterTable;
        await connPragma.LegacyAlterTableAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.LegacyAlterTableAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void ModuleListAsync_WhenInvoked_ThrowsNothing()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.ModuleListAsync().ConfigureAwait(false), Throws.Nothing);
    }

    [Test]
    public static void OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.OptimizeAsync().ConfigureAwait(false), Throws.Nothing);
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

        Assert.That(async () => await connPragma.PragmaListAsync().ConfigureAwait(false), Throws.Nothing);
    }

    [Test]
    public static async Task QueryOnlyAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var queryOnly = await connPragma.QueryOnlyAsync().ConfigureAwait(false);
        var newValue = !queryOnly;
        await connPragma.QueryOnlyAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.QueryOnlyAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task ReadUncommittedAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var readUncommitted = await connPragma.ReadUncommittedAsync().ConfigureAwait(false);
        var newValue = !readUncommitted;
        await connPragma.ReadUncommittedAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.ReadUncommittedAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task RecursiveTriggersAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var recursiveTriggers = await connPragma.RecursiveTriggersAsync().ConfigureAwait(false);
        var newValue = !recursiveTriggers;
        await connPragma.RecursiveTriggersAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.RecursiveTriggersAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task ReverseUnorderedSelectsAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var recursiveTriggers = await connPragma.ReverseUnorderedSelectsAsync().ConfigureAwait(false);
        var newValue = !recursiveTriggers;
        await connPragma.ReverseUnorderedSelectsAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.ReverseUnorderedSelectsAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void ShrinkMemoryAsync_WhenInvoked_PerformsOperationSuccessfully()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        Assert.That(async () => await connPragma.ShrinkMemoryAsync().ConfigureAwait(false), Throws.Nothing);
    }

    [Test]
    public static async Task SoftHeapLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var softHeapLimit = await connPragma.SoftHeapLimitAsync().ConfigureAwait(false);
        var newValue = softHeapLimit == 0 ? 9000 : 0;
        await connPragma.SoftHeapLimitAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.SoftHeapLimitAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task TableListAsync_WhenTableExists_ReadsValuesCorrectlyForDatabase()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);

        var tableInfo = await connPragma.TableListAsync().ConfigureAwait(false);

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static async Task TemporaryStoreAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var tempStore = await connPragma.TemporaryStoreAsync().ConfigureAwait(false);
        var newValue = tempStore == TemporaryStoreLocation.Default ? TemporaryStoreLocation.File : TemporaryStoreLocation.Default;
        await connPragma.TemporaryStoreAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.TemporaryStoreAsync().ConfigureAwait(false);

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

        var threads = await connPragma.ThreadsAsync().ConfigureAwait(false);
        var newValue = threads == 0 ? 8 : 0;
        await connPragma.ThreadsAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.ThreadsAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task WalAutoCheckpointAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var autoCheckpoint = await connPragma.WalAutoCheckpointAsync().ConfigureAwait(false);
        var newValue = autoCheckpoint == 50 ? 100 : 50;
        await connPragma.WalAutoCheckpointAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.WalAutoCheckpointAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task WritableSchemaAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);

        var writableSchema = await connPragma.WritableSchemaAsync().ConfigureAwait(false);
        var newValue = !writableSchema;
        await connPragma.WritableSchemaAsync(newValue).ConfigureAwait(false);
        var readOfNewValue = await connPragma.WritableSchemaAsync().ConfigureAwait(false);

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }
}