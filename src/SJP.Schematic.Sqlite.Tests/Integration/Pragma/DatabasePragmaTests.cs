using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration.Pragma;

internal sealed class DatabasePragmaTests : SqliteTest
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

    private static ISqliteDatabasePragma CreateDatabasePragma(IDbConnectionFactory connectionFactory, string schemaName)
    {
        var conn = new SchematicConnection(Guid.NewGuid(), connectionFactory, new SqliteDialect());
        return new DatabasePragma(conn, schemaName);
    }

    private const string MainSchema = "main";

    [Test]
    public static async Task ApplicationIdAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var applicationId = await dbPragma.ApplicationIdAsync();
        var newValue = applicationId == 123 ? 456 : 123;
        await dbPragma.ApplicationIdAsync(newValue);
        var readOfNewValue = await dbPragma.ApplicationIdAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task AutoVacuumAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var autoVacuum = await dbPragma.AutoVacuumAsync();
        var newValue = autoVacuum == AutoVacuumMode.Disabled ? AutoVacuumMode.Incremental : AutoVacuumMode.Disabled;
        await dbPragma.AutoVacuumAsync(newValue);
        var readOfNewValue = await dbPragma.AutoVacuumAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void AutoVacuumAsync_GivenInvalidAutoVacuumModeValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const AutoVacuumMode newValue = (AutoVacuumMode)55;
        Assert.That(() => dbPragma.AutoVacuumAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task CacheSizeInPagesAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var cacheSizeInPages = await dbPragma.CacheSizeInPagesAsync();
        var newValue = cacheSizeInPages == 1000u ? 2000u : 1000u;
        await dbPragma.CacheSizeInPagesAsync(newValue);
        var readOfNewValue = await dbPragma.CacheSizeInPagesAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CacheSizeInKibibytesAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var cacheSizeInKibibytes = await dbPragma.CacheSizeInKibibytesAsync();
        var newValue = cacheSizeInKibibytes == 1000u ? 2000u : 1000u;
        await dbPragma.CacheSizeInKibibytesAsync(newValue);
        var readOfNewValue = await dbPragma.CacheSizeInKibibytesAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task CacheSpillAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var cacheSpill = await dbPragma.CacheSpillAsync();
        var newValue = !cacheSpill;
        await dbPragma.CacheSpillAsync(newValue);
        var readOfNewValue = await dbPragma.CacheSpillAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void DataVersionAsync_WhenGetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.DataVersionAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task ForeignKeyCheckDatabaseAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);
        await connPragma.ForeignKeysAsync(false); // must disable enforcement to allow delayed check

        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)", CancellationToken.None);

        var fkCheck = await dbPragma.ForeignKeyCheckDatabaseAsync();

        Assert.That(fkCheck, Is.Not.Empty);
    }

    [Test]
    public static async Task ForeignKeyCheckTableAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var connPragma = CreateConnectionPragma(connection);
        await connPragma.ForeignKeysAsync(false); // must disable enforcement to allow delayed check

        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)", CancellationToken.None);

        var fkCheck = await dbPragma.ForeignKeyCheckTableAsync("test_child");

        Assert.That(fkCheck, Is.Not.Empty);
    }

    [Test]
    public static void ForeignKeyCheckTableAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.ForeignKeyCheckTableAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ForeignKeyCheckTableAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.ForeignKeyCheckTableAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static async Task ForeignKeyListAsync_WhenTableExists_ReadsKeysCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None);
        await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 1)", CancellationToken.None);

        var fkList = await dbPragma.ForeignKeyListAsync("test_child");

        Assert.That(fkList, Is.Not.Empty);
    }

    [Test]
    public static void ForeignKeyListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.ForeignKeyListAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ForeignKeyListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.ForeignKeyListAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static void FreeListCountAsync_WhenGetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.FreeListCountAsync(), Throws.Nothing);
    }

    [Test]
    public static void IncrementalVacuumAsync_GivenNonZeroValue_SetsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.IncrementalVacuumAsync(1000), Throws.Nothing);
    }

    [Test]
    public static void IncrementalVacuumAsync_GivenZeroValue_SetsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.IncrementalVacuumAsync(0), Throws.Nothing);
    }

    [Test]
    public static async Task IndexInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None);

        var indexInfos = await dbPragma.IndexInfoAsync("ix_test_index");
        var indexInfo = indexInfos.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexInfo, Is.Not.Null);
            Assert.That(indexInfo.name, Is.EqualTo("val")); // first column
        }
    }

    [Test]
    public static async Task IndexListAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None);

        var indexList = await dbPragma.IndexListAsync("test_table");
        var firstIndex = indexList.First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstIndex, Is.Not.Null);
            Assert.That(firstIndex.name, Is.EqualTo("ix_test_index"));
        }
    }

    [Test]
    public static void IndexListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.IndexListAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void IndexListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.IndexListAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static async Task IndexXInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);
        await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None);

        var indexXInfos = await dbPragma.IndexXInfoAsync("ix_test_index");
        var indexXInfo = indexXInfos.First(info => info.cid >= 0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexXInfo, Is.Not.Null);
            Assert.That(indexXInfo.name, Is.EqualTo("val")); // first column
        }
    }

    [Test]
    public static async Task IntegrityCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var errors = await dbPragma.IntegrityCheckAsync();

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task IntegrityCheckAsync_GivenZeroErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var errors = await dbPragma.IntegrityCheckAsync(0);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task IntegrityCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var errors = await dbPragma.IntegrityCheckAsync(10);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task IntegrityCheckAsync_GivenNoErrorsForTableCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var errors = await dbPragma.IntegrityCheckAsync("test_table");

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task JournalModeAsync_WhenGetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var journalMode = await dbPragma.JournalModeAsync();

        Assert.That(journalMode, Is.EqualTo(JournalMode.Memory));
    }

    [Test]
    [Ignore("Can't change journaling mode for an in-memory db")]
    public static async Task JournalModeAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var journalMode = await dbPragma.JournalModeAsync();
        var newValue = journalMode == JournalMode.Persist ? JournalMode.Memory : JournalMode.Persist;
        await dbPragma.JournalModeAsync(newValue);
        var readOfNewValue = await dbPragma.JournalModeAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void JournalModeAsync_GivenInvalidJournalModeValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const JournalMode newValue = (JournalMode)55;
        Assert.That(() => dbPragma.JournalModeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task JournalSizeLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var journalSizeLimit = await dbPragma.JournalSizeLimitAsync();
        var newValue = journalSizeLimit == 123u ? 456u : 123u;
        await dbPragma.JournalSizeLimitAsync(newValue);
        var readOfNewValue = await dbPragma.JournalSizeLimitAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task LockingModeAsync_GetAndSet_InvokesProperly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        _ = await dbPragma.LockingModeAsync(); // should be normal
        const LockingMode newValue = LockingMode.Exclusive;
        await dbPragma.LockingModeAsync(newValue);
        // not checking value as it's a once-only effect
        Assert.That(async () => _ = await dbPragma.LockingModeAsync(), Throws.Nothing);
    }

    [Test]
    public static void LockingModeAsync_GivenInvalidLockingModeValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const LockingMode newValue = (LockingMode)55;
        Assert.That(() => dbPragma.LockingModeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task MaxPageCountAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var maxPageCount = await dbPragma.MaxPageCountAsync();
        var newValue = maxPageCount == 123u ? 456u : 123u;
        await dbPragma.MaxPageCountAsync(newValue);
        var readOfNewValue = await dbPragma.MaxPageCountAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    [Ignore("Not using an mmaped database for testing")]
    public static async Task MmapSizeAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var mmapSize = await dbPragma.MmapSizeAsync();
        var newValue = mmapSize == 123u ? 456u : 123u;
        await dbPragma.MmapSizeAsync(newValue);
        var readOfNewValue = await dbPragma.MmapSizeAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.OptimizeAsync(), Throws.Nothing);
    }

    [Test]
    public static void OptimizeAsync_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const OptimizeFeatures newValue = (OptimizeFeatures)55;
        Assert.That(() => dbPragma.OptimizeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static void PageCountAsync_WhenGetInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(async () => await dbPragma.PageCountAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task PageSize_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var pageSize = await dbPragma.PageSizeAsync();
        var newValue = pageSize == (ushort)512u ? (ushort)1024u : (ushort)512u;
        await dbPragma.PageSizeAsync(newValue);
        var readOfNewValue = await dbPragma.PageSizeAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void PageSizeAsync_WhenSetValueLessThan512_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const ushort newValue = 300;
        Assert.That(() => dbPragma.PageSizeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static void PageSizeAsync_WhenSetValueNotPowerOfTwo_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const ushort newValue = 600;
        Assert.That(() => dbPragma.PageSizeAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task QuickCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var errors = await dbPragma.QuickCheckAsync();

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task QuickCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var errors = await dbPragma.QuickCheckAsync(10);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public static async Task SchemaVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var schemaVersion = await dbPragma.SchemaVersionAsync();
        var newValue = schemaVersion == 123 ? 456 : 123;
        await dbPragma.SchemaVersionAsync(newValue);
        var readOfNewValue = await dbPragma.SchemaVersionAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task SecureDeleteAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var secureDelete = await dbPragma.SecureDeleteAsync();
        var newValue = secureDelete == SecureDeleteMode.On ? SecureDeleteMode.Off : SecureDeleteMode.On;
        await dbPragma.SecureDeleteAsync(newValue);
        var readOfNewValue = await dbPragma.SecureDeleteAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void SecureDeleteAsync_GivenInvalidSecureDeleteModeValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const SecureDeleteMode newValue = (SecureDeleteMode)55;
        Assert.That(() => dbPragma.SecureDeleteAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task SynchronousAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var synchronous = await dbPragma.SynchronousAsync();
        var newValue = synchronous == SynchronousLevel.Normal ? SynchronousLevel.Full : SynchronousLevel.Normal;
        await dbPragma.SynchronousAsync(newValue);
        var readOfNewValue = await dbPragma.SynchronousAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static void SynchronousAsync_GivenInvalidSynchronousLevelValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const SynchronousLevel newValue = (SynchronousLevel)55;
        Assert.That(() => dbPragma.SynchronousAsync(newValue), Throws.ArgumentException);
    }

    [Test]
    public static async Task TableInfoAsync_WhenTableExists_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var tableInfo = await dbPragma.TableInfoAsync("test_table");

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static void TableInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.TableInfoAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.TableInfoAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static async Task TableListAsync_WhenTableExists_ReadsValuesCorrectlyForSchema()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var tableInfo = await dbPragma.TableListAsync();

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static async Task TableListAsync_WhenTableExists_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var tableInfo = await dbPragma.TableListAsync("test_table");

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static async Task TableListAsync_WhenViewExists_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create view test_view as select 1 as dummy", CancellationToken.None);

        var tableInfo = await dbPragma.TableListAsync("test_view");

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static void TableListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.TableListAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.TableListAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static async Task TableXInfoAsync_WhenTableExists_ReadsValuesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None);

        var tableInfo = await dbPragma.TableXInfoAsync("test_table");

        Assert.That(tableInfo, Is.Not.Empty);
    }

    [Test]
    public static void TableXInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        Assert.That(() => dbPragma.TableXInfoAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableXInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var name = new Identifier("test", "test");
        Assert.That(() => dbPragma.TableXInfoAsync(name), Throws.ArgumentException);
    }

    [Test]
    public static async Task UserVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        var userVersion = await dbPragma.UserVersionAsync();
        var newValue = userVersion == 123 ? 456 : 123;
        await dbPragma.UserVersionAsync(newValue);
        var readOfNewValue = await dbPragma.UserVersionAsync();

        Assert.That(readOfNewValue, Is.EqualTo(newValue));
    }

    [Test]
    public static async Task WalCheckpointAsync_WhenInvoked_ReadsCorrectly()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);
        var results = await dbPragma.WalCheckpointAsync();

        Assert.That(results, Is.Not.Null);
    }

    [Test]
    public static void WalCheckpointAsync_GivenInvalidWalCheckpointModeValue_ThrowsArgumentException()
    {
        var connection = CreateConnectionFactory();
        var dbPragma = CreateDatabasePragma(connection, MainSchema);

        const WalCheckpointMode newValue = (WalCheckpointMode)55;
        Assert.That(() => dbPragma.WalCheckpointAsync(newValue), Throws.ArgumentException);
    }
}