using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration.Pragma
{
    internal sealed class DatabasePragmaTests : SqliteTest
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

        private static ISqliteDatabasePragma CreateDatabasePragma(IDbConnectionFactory connectionFactory, string schemaName)
        {
            var conn = new SchematicConnection(Guid.NewGuid(), connectionFactory, new SqliteDialect());
            return new DatabasePragma(conn, schemaName);
        }

        private const string MainSchema = "main";

        [Test]
        public async Task ApplicationIdAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var applicationId = await dbPragma.ApplicationIdAsync().ConfigureAwait(false);
            var newValue = applicationId == 123 ? 456 : 123;
            await dbPragma.ApplicationIdAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.ApplicationIdAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task AutoVacuumAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var autoVacuum = await dbPragma.AutoVacuumAsync().ConfigureAwait(false);
            var newValue = autoVacuum == AutoVacuumMode.Disabled ? AutoVacuumMode.Incremental : AutoVacuumMode.Disabled;
            await dbPragma.AutoVacuumAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.AutoVacuumAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void AutoVacuumAsync_GivenInvalidAutoVacuumModeValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const AutoVacuumMode newValue = (AutoVacuumMode)55;
            Assert.That(() => dbPragma.AutoVacuumAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task CacheSizeInPagesAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var cacheSizeInPages = await dbPragma.CacheSizeInPagesAsync().ConfigureAwait(false);
            var newValue = cacheSizeInPages == 1000u ? 2000u : 1000u;
            await dbPragma.CacheSizeInPagesAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.CacheSizeInPagesAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task CacheSizeInKibibytesAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var cacheSizeInKibibytes = await dbPragma.CacheSizeInKibibytesAsync().ConfigureAwait(false);
            var newValue = cacheSizeInKibibytes == 1000u ? 2000u : 1000u;
            await dbPragma.CacheSizeInKibibytesAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.CacheSizeInKibibytesAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task CacheSpillAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var cacheSpill = await dbPragma.CacheSpillAsync().ConfigureAwait(false);
            var newValue = !cacheSpill;
            await dbPragma.CacheSpillAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.CacheSpillAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void DataVersionAsync_WhenGetInvoked_ReadsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.DataVersionAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public async Task ForeignKeyCheckDatabaseAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var connPragma = CreateConnectionPragma(connection);
            await connPragma.ForeignKeysAsync(false).ConfigureAwait(false); // must disable enforcement to allow delayed check

            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)", CancellationToken.None).ConfigureAwait(false);

            var fkCheck = await dbPragma.ForeignKeyCheckDatabaseAsync().ConfigureAwait(false);

            Assert.That(fkCheck, Is.Not.Empty);
        }

        [Test]
        public async Task ForeignKeyCheckTableAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var connPragma = CreateConnectionPragma(connection);
            await connPragma.ForeignKeysAsync(false).ConfigureAwait(false); // must disable enforcement to allow delayed check

            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)", CancellationToken.None).ConfigureAwait(false);

            var fkCheck = await dbPragma.ForeignKeyCheckTableAsync("test_child").ConfigureAwait(false);

            Assert.That(fkCheck, Is.Not.Empty);
        }

        [Test]
        public void ForeignKeyCheckTableAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(() => dbPragma.ForeignKeyCheckTableAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ForeignKeyCheckTableAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var name = new Identifier("test", "test");
            Assert.That(() => dbPragma.ForeignKeyCheckTableAsync(name), Throws.ArgumentException);
        }

        [Test]
        public async Task ForeignKeyListAsync_WhenTableExists_ReadsKeysCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'test')", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 1)", CancellationToken.None).ConfigureAwait(false);

            var fkList = await dbPragma.ForeignKeyListAsync("test_child").ConfigureAwait(false);

            Assert.That(fkList, Is.Not.Empty);
        }

        [Test]
        public void ForeignKeyListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(() => dbPragma.ForeignKeyListAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ForeignKeyListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var name = new Identifier("test", "test");
            Assert.That(() => dbPragma.ForeignKeyListAsync(name), Throws.ArgumentException);
        }

        [Test]
        public void FreeListCountAsync_WhenGetInvoked_ReadsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.FreeListCountAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void IncrementalVacuumAsync_GivenNonZeroValue_SetsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.IncrementalVacuumAsync(1000).ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void IncrementalVacuumAsync_GivenZeroValue_SetsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.IncrementalVacuumAsync(0).ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public async Task IndexInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None).ConfigureAwait(false);

            var indexInfos = await dbPragma.IndexInfoAsync("ix_test_index").ConfigureAwait(false);
            var indexInfo = indexInfos.Single();

            Assert.Multiple(() =>
            {
                Assert.That(indexInfo, Is.Not.Null);
                Assert.That(indexInfo.name, Is.EqualTo("val")); // first column
            });
        }

        [Test]
        public async Task IndexListAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None).ConfigureAwait(false);

            var indexList = await dbPragma.IndexListAsync("test_table").ConfigureAwait(false);
            var firstIndex = indexList.First();

            Assert.Multiple(() =>
            {
                Assert.That(firstIndex, Is.Not.Null);
                Assert.That(firstIndex.name, Is.EqualTo("ix_test_index"));
            });
        }

        [Test]
        public void IndexListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(() => dbPragma.IndexListAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void IndexListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var name = new Identifier("test", "test");
            Assert.That(() => dbPragma.IndexListAsync(name), Throws.ArgumentException);
        }

        [Test]
        public async Task IndexXInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);
            await connection.ExecuteAsync("create index ix_test_index on test_table (val)", CancellationToken.None).ConfigureAwait(false);

            var indexXInfos = await dbPragma.IndexXInfoAsync("ix_test_index").ConfigureAwait(false);
            var indexXInfo = indexXInfos.First(info => info.cid >= 0);

            Assert.Multiple(() =>
            {
                Assert.That(indexXInfo, Is.Not.Null);
                Assert.That(indexXInfo.name, Is.EqualTo("val")); // first column
            });
        }

        [Test]
        public async Task IntegrityCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var errors = await dbPragma.IntegrityCheckAsync().ConfigureAwait(false);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public async Task IntegrityCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var errors = await dbPragma.IntegrityCheckAsync(10).ConfigureAwait(false);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public static async Task JournalModeAsync_WhenGetInvoked_ReadsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var journalMode = await dbPragma.JournalModeAsync().ConfigureAwait(false);

            Assert.That(journalMode, Is.EqualTo(JournalMode.Memory));
        }

        [Test]
        [Ignore("Can't change journaling mode for an in-memory db")]
        public async Task JournalModeAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var journalMode = await dbPragma.JournalModeAsync().ConfigureAwait(false);
            var newValue = journalMode == JournalMode.Persist ? JournalMode.Memory : JournalMode.Persist;
            await dbPragma.JournalModeAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.JournalModeAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void JournalModeAsync_GivenInvalidJournalModeValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const JournalMode newValue = (JournalMode)55;
            Assert.That(() => dbPragma.JournalModeAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task JournalSizeLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var journalSizeLimit = await dbPragma.JournalSizeLimitAsync().ConfigureAwait(false);
            var newValue = journalSizeLimit == 123u ? 456u : 123u;
            await dbPragma.JournalSizeLimitAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.JournalSizeLimitAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task LockingModeAsync_GetAndSet_InvokesProperly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            _ = await dbPragma.LockingModeAsync().ConfigureAwait(false); // should be normal
            const LockingMode newValue = LockingMode.Exclusive;
            await dbPragma.LockingModeAsync(newValue).ConfigureAwait(false);
            // not checking value as it's a once-only effect
            Assert.That(async () => _ = await dbPragma.LockingModeAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void LockingModeAsync_GivenInvalidLockingModeValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const LockingMode newValue = (LockingMode)55;
            Assert.That(() => dbPragma.LockingModeAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task MaxPageCountAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var maxPageCount = await dbPragma.MaxPageCountAsync().ConfigureAwait(false);
            var newValue = maxPageCount == 123u ? 456u : 123u;
            await dbPragma.MaxPageCountAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.MaxPageCountAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        [Ignore("Not using an mmaped database for testing")]
        public async Task MmapSizeAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var mmapSize = await dbPragma.MmapSizeAsync().ConfigureAwait(false);
            var newValue = mmapSize == 123u ? 456u : 123u;
            await dbPragma.MmapSizeAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.MmapSizeAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.OptimizeAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void OptimizeAsync_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const OptimizeFeatures newValue = (OptimizeFeatures)55;
            Assert.That(() => dbPragma.OptimizeAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public void PageCountAsync_WhenGetInvoked_ReadsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(async () => await dbPragma.PageCountAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public async Task PageSize_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var pageSize = await dbPragma.PageSizeAsync().ConfigureAwait(false);
            var newValue = pageSize == (ushort)512u ? (ushort)1024u : (ushort)512u;
            await dbPragma.PageSizeAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.PageSizeAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void PageSizeAsync_WhenSetValueLessThan512_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const ushort newValue = 300;
            Assert.That(() => dbPragma.PageSizeAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public void PageSizeAsync_WhenSetValueNotPowerOfTwo_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const ushort newValue = 600;
            Assert.That(() => dbPragma.PageSizeAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task QuickCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var errors = await dbPragma.QuickCheckAsync().ConfigureAwait(false);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public async Task QuickCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var errors = await dbPragma.QuickCheckAsync(10).ConfigureAwait(false);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public async Task SchemaVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var schemaVersion = await dbPragma.SchemaVersionAsync().ConfigureAwait(false);
            var newValue = schemaVersion == 123 ? 456 : 123;
            await dbPragma.SchemaVersionAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.SchemaVersionAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task SecureDeleteAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var secureDelete = await dbPragma.SecureDeleteAsync().ConfigureAwait(false);
            var newValue = secureDelete == SecureDeleteMode.On ? SecureDeleteMode.Off : SecureDeleteMode.On;
            await dbPragma.SecureDeleteAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.SecureDeleteAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void SecureDeleteAsync_GivenInvalidSecureDeleteModeValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const SecureDeleteMode newValue = (SecureDeleteMode)55;
            Assert.That(() => dbPragma.SecureDeleteAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task SynchronousAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var synchronous = await dbPragma.SynchronousAsync().ConfigureAwait(false);
            var newValue = synchronous == SynchronousLevel.Normal ? SynchronousLevel.Full : SynchronousLevel.Normal;
            await dbPragma.SynchronousAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.SynchronousAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public void SynchronousAsync_GivenInvalidSynchronousLevelValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const SynchronousLevel newValue = (SynchronousLevel)55;
            Assert.That(() => dbPragma.SynchronousAsync(newValue), Throws.ArgumentException);
        }

        [Test]
        public async Task TableInfoAsync_WhenTableExists_ReadsValuesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);

            var tableInfo = await dbPragma.TableInfoAsync("test_table").ConfigureAwait(false);

            Assert.That(tableInfo, Is.Not.Empty);
        }

        [Test]
        public void TableInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(() => dbPragma.TableInfoAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void TableInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var name = new Identifier("test", "test");
            Assert.That(() => dbPragma.TableInfoAsync(name), Throws.ArgumentException);
        }

        [Test]
        public async Task TableXInfoAsync_WhenTableExists_ReadsValuesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            await connection.ExecuteAsync("create table test_table ( id int primary key, val text )", CancellationToken.None).ConfigureAwait(false);

            var tableInfo = await dbPragma.TableXInfoAsync("test_table").ConfigureAwait(false);

            Assert.That(tableInfo, Is.Not.Empty);
        }

        [Test]
        public void TableXInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            Assert.That(() => dbPragma.TableXInfoAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public void TableXInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var name = new Identifier("test", "test");
            Assert.That(() => dbPragma.TableXInfoAsync(name), Throws.ArgumentException);
        }

        [Test]
        public async Task UserVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            var userVersion = await dbPragma.UserVersionAsync().ConfigureAwait(false);
            var newValue = userVersion == 123 ? 456 : 123;
            await dbPragma.UserVersionAsync(newValue).ConfigureAwait(false);
            var readOfNewValue = await dbPragma.UserVersionAsync().ConfigureAwait(false);

            Assert.That(readOfNewValue, Is.EqualTo(newValue));
        }

        [Test]
        public async Task WalCheckpointAsync_WhenInvoked_ReadsCorrectly()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);
            var results = await dbPragma.WalCheckpointAsync().ConfigureAwait(false);

            Assert.That(results, Is.Not.Null);
        }

        [Test]
        public void WalCheckpointAsync_GivenInvalidWalCheckpointModeValue_ThrowsArgumentException()
        {
            var connection = CreateConnectionFactory();
            var dbPragma = CreateDatabasePragma(connection, MainSchema);

            const WalCheckpointMode newValue = (WalCheckpointMode)55;
            Assert.That(() => dbPragma.WalCheckpointAsync(newValue), Throws.ArgumentException);
        }
    }
}
