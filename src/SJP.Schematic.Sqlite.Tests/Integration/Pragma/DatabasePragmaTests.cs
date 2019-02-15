using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Dapper;
using SJP.Schematic.Core;
using System.Linq;
using System.Data;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration.Pragma
{
    internal sealed class DatabasePragmaTests : SqliteTest
    {
        private static IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            return connection;
        }

        private const string MainSchema = "main";

        [Test]
        public async Task ApplicationIdAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var applicationId = await dbPragma.ApplicationIdAsync().ConfigureAwait(false);
                var newValue = applicationId == 123u ? 456u : 123u;
                await dbPragma.ApplicationIdAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.ApplicationIdAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task AutoVacuumAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var autoVacuum = await dbPragma.AutoVacuumAsync().ConfigureAwait(false);
                var newValue = autoVacuum == AutoVacuumMode.None ? AutoVacuumMode.Incremental : AutoVacuumMode.None;
                await dbPragma.AutoVacuumAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.AutoVacuumAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void AutoVacuumAsync_GivenInvalidAutoVacuumModeValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const AutoVacuumMode newValue = (AutoVacuumMode)55;
                Assert.Throws<ArgumentException>(() => dbPragma.AutoVacuumAsync(newValue));
            }
        }

        [Test]
        public async Task CacheSizeInPagesAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var cacheSizeInPages = await dbPragma.CacheSizeInPagesAsync().ConfigureAwait(false);
                var newValue = cacheSizeInPages == 1000u ? 2000u : 1000u;
                await dbPragma.CacheSizeInPagesAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.CacheSizeInPagesAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task CacheSizeInKibibytesAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var cacheSizeInKibibytes = await dbPragma.CacheSizeInKibibytesAsync().ConfigureAwait(false);
                var newValue = cacheSizeInKibibytes == 1000u ? 2000u : 1000u;
                await dbPragma.CacheSizeInKibibytesAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.CacheSizeInKibibytesAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task CacheSpillAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var cacheSpill = await dbPragma.CacheSpillAsync().ConfigureAwait(false);
                var newValue = !cacheSpill;
                await dbPragma.CacheSpillAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.CacheSpillAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void DataVersionAsync_WhenGetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.DoesNotThrowAsync(() => dbPragma.DataVersionAsync());
            }
        }

        [Test]
        public async Task ForeignKeyCheckDatabaseAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var connPragma = new ConnectionPragma(Dialect, connection);
                await connPragma.ForeignKeysAsync(false).ConfigureAwait(false); // must disable enforcement to allow delayed check

                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'asd')").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)").ConfigureAwait(false);

                var fkCheck = await dbPragma.ForeignKeyCheckDatabaseAsync().ConfigureAwait(false);
                var fkCheckCount = fkCheck.Count();

                Assert.NotZero(fkCheckCount);
            }
        }

        [Test]
        public async Task ForeignKeyCheckTableAsync_WhenBrokenRelationshipsExist_ReadsValuesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var connPragma = new ConnectionPragma(Dialect, connection);
                await connPragma.ForeignKeysAsync(false).ConfigureAwait(false); // must disable enforcement to allow delayed check

                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'asd')").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 2)").ConfigureAwait(false);

                var fkCheck = await dbPragma.ForeignKeyCheckTableAsync("test_child").ConfigureAwait(false);
                var fkCheckCount = fkCheck.Count();

                Assert.NotZero(fkCheckCount);
            }
        }

        [Test]
        public void ForeignKeyCheckTableAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.Throws<ArgumentNullException>(() => dbPragma.ForeignKeyCheckTableAsync(null));
            }
        }

        [Test]
        public void ForeignKeyCheckTableAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var name = new Identifier("aksldjaslk", "asjdkas");
                Assert.Throws<ArgumentException>(() => dbPragma.ForeignKeyCheckTableAsync(name));
            }
        }

        [Test]
        public async Task ForeignKeyListAsync_WhenTableExists_ReadsKeysCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_parent ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create table test_child ( id int, parent_id int constraint fk_test_parent references test_parent (id) )").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_parent (id, val) values (1, 'asd')").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test_child (id, parent_id) values (1, 1)").ConfigureAwait(false);

                var fkList = await dbPragma.ForeignKeyListAsync("test_child").ConfigureAwait(false);
                var fkListCount = fkList.Count();

                Assert.NotZero(fkListCount);
            }
        }

        [Test]
        public void ForeignKeyListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.Throws<ArgumentNullException>(() => dbPragma.ForeignKeyListAsync(null));
            }
        }

        [Test]
        public void ForeignKeyListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var name = new Identifier("aksldjaslk", "asjdkas");
                Assert.Throws<ArgumentException>(() => dbPragma.ForeignKeyListAsync(name));
            }
        }

        [Test]
        public void FreeListCountAsync_WhenGetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.DoesNotThrowAsync(() => dbPragma.FreeListCountAsync());
            }
        }

        [Test]
        public async Task IncrementalVacuumAsync_GivenNonZeroValue_SetsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await dbPragma.IncrementalVacuumAsync(1000).ConfigureAwait(false);
                Assert.Pass();
            }
        }

        [Test]
        public async Task IncrementalVacuumAsync_GivenZeroValue_SetsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await dbPragma.IncrementalVacuumAsync(0).ConfigureAwait(false);
                Assert.Pass();
            }
        }

        [Test]
        public async Task IndexInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_table ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create index ix_test_index on test_table (val)").ConfigureAwait(false);

                var indexInfos = await dbPragma.IndexInfoAsync("ix_test_index").ConfigureAwait(false);
                var indexInfo = indexInfos.Single();

                Assert.Multiple(() =>
                {
                    Assert.IsNotNull(indexInfo);
                    Assert.AreEqual("val", indexInfo.name); // first column
                });
            }
        }

        [Test]
        public async Task IndexListAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_table ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create index ix_test_index on test_table (val)").ConfigureAwait(false);

                var indexList = await dbPragma.IndexListAsync("test_table").ConfigureAwait(false);
                var firstIndex = indexList.First();

                Assert.Multiple(() =>
                {
                    Assert.IsNotNull(firstIndex);
                    Assert.AreEqual("ix_test_index", firstIndex.name);
                });
            }
        }

        [Test]
        public void IndexListAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.Throws<ArgumentNullException>(() => dbPragma.IndexListAsync(null));
            }
        }

        [Test]
        public void IndexListAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var name = new Identifier("aksldjaslk", "asjdkas");
                Assert.Throws<ArgumentException>(() => dbPragma.IndexListAsync(name));
            }
        }

        [Test]
        public async Task IndexXInfoAsync_WhenIndexOnTableExists_ReadsIndexCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await connection.ExecuteAsync("create table test_table ( id int primary key, val text )").ConfigureAwait(false);
                await connection.ExecuteAsync("create index ix_test_index on test_table (val)").ConfigureAwait(false);

                var indexXInfos = await dbPragma.IndexXInfoAsync("ix_test_index").ConfigureAwait(false);
                var indexXInfo = indexXInfos.First(info => info.cid >= 0);

                Assert.Multiple(() =>
                {
                    Assert.IsNotNull(indexXInfo);
                    Assert.AreEqual("val", indexXInfo.name); // first column
                });
            }
        }

        [Test]
        public async Task IntegrityCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var errors = await dbPragma.IntegrityCheckAsync().ConfigureAwait(false);
                var errorCount = errors.Count();

                Assert.Zero(errorCount);
            }
        }

        [Test]
        public async Task IntegrityCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var errors = await dbPragma.IntegrityCheckAsync(10).ConfigureAwait(false);
                var errorCount = errors.Count();

                Assert.Zero(errorCount);
            }
        }

        public async Task JournalModeAsync_WhenGetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var journalMode = await dbPragma.JournalModeAsync().ConfigureAwait(false);

                Assert.AreEqual(JournalMode.Memory, journalMode);
            }
        }

        [Test]
        [Ignore("Can't change journaling mode for an in-memory db")]
        public async Task JournalModeAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var journalMode = await dbPragma.JournalModeAsync().ConfigureAwait(false);
                var newValue = journalMode == JournalMode.Persist ? JournalMode.Memory : JournalMode.Persist;
                await dbPragma.JournalModeAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.JournalModeAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void JournalModeAsync_GivenInvalidJournalModeValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const JournalMode newValue = (JournalMode)55;
                Assert.Throws<ArgumentException>(() => dbPragma.JournalModeAsync(newValue));
            }
        }

        [Test]
        public async Task JournalSizeLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var journalSizeLimit = await dbPragma.JournalSizeLimitAsync().ConfigureAwait(false);
                var newValue = journalSizeLimit == 123u ? 456u : 123u;
                await dbPragma.JournalSizeLimitAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.JournalSizeLimitAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task LockingModeAsync_GetAndSet_InvokesProperly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                _ = await dbPragma.LockingModeAsync().ConfigureAwait(false); // should be normal
                const LockingMode newValue = LockingMode.Exclusive;
                await dbPragma.LockingModeAsync(newValue).ConfigureAwait(false);
                _ = await dbPragma.LockingModeAsync().ConfigureAwait(false);

                Assert.Pass(); // not checking value as it's a once-only effect
            }
        }

        [Test]
        public void LockingModeAsync_GivenInvalidLockingModeValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const LockingMode newValue = (LockingMode)55;
                Assert.Throws<ArgumentException>(() => dbPragma.LockingModeAsync(newValue));
            }
        }

        [Test]
        public async Task MaxPageCountAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var maxPageCount = await dbPragma.MaxPageCountAsync().ConfigureAwait(false);
                var newValue = maxPageCount == 123u ? 456u : 123u;
                await dbPragma.MaxPageCountAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.MaxPageCountAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        [Ignore("Not using an mmaped database for testing")]
        public async Task MmapSizeAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var mmapSize = await dbPragma.MmapSizeAsync().ConfigureAwait(false);
                var newValue = mmapSize == 123u ? 456u : 123u;
                await dbPragma.MmapSizeAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.MmapSizeAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                await dbPragma.OptimizeAsync().ConfigureAwait(false);
                Assert.Pass();
            }
        }

        [Test]
        public void OptimizeAsync_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const OptimizeFeatures newValue = (OptimizeFeatures)55;
                Assert.Throws<ArgumentException>(() => dbPragma.OptimizeAsync(newValue));
            }
        }

        [Test]
        public void PageCountAsync_WhenGetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.DoesNotThrowAsync(() => dbPragma.PageCountAsync());
            }
        }

        [Test]
        public async Task PageSize_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var pageSize = await dbPragma.PageSizeAsync().ConfigureAwait(false);
                var newValue = pageSize == (ushort)512u ? (ushort)1024u : (ushort)512u;
                await dbPragma.PageSizeAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.PageSizeAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void PageSizeAsync_WhenSetValueLessThan512_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const ushort newValue = 300;
                Assert.Throws<ArgumentException>(() => dbPragma.PageSizeAsync(newValue));
            }
        }

        [Test]
        public void PageSizeAsync_WhenSetValueNotPowerOfTwo_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const ushort newValue = 600;
                Assert.Throws<ArgumentException>(() => dbPragma.PageSizeAsync(newValue));
            }
        }

        [Test]
        public async Task QuickCheckAsync_GivenNoMaxErrorsOnCorrectDb_ReturnsEmptyCollection()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var errors = await dbPragma.QuickCheckAsync().ConfigureAwait(false);
                var errorCount = errors.Count();

                Assert.Zero(errorCount);
            }
        }

        [Test]
        public async Task QuickCheckAsync_GivenMaxErrorsLimitOnCorrectDb_ReturnsEmptyCollection()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var errors = await dbPragma.QuickCheckAsync(10).ConfigureAwait(false);
                var errorCount = errors.Count();

                Assert.Zero(errorCount);
            }
        }

        [Test]
        public async Task SchemaVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var schemaVersion = await dbPragma.SchemaVersionAsync().ConfigureAwait(false);
                var newValue = schemaVersion == 123 ? 456 : 123;
                await dbPragma.SchemaVersionAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.SchemaVersionAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task SecureDeleteAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var secureDelete = await dbPragma.SecureDeleteAsync().ConfigureAwait(false);
                var newValue = secureDelete == SecureDeleteMode.On ? SecureDeleteMode.Off : SecureDeleteMode.On;
                await dbPragma.SecureDeleteAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.SecureDeleteAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void SecureDeleteAsync_GivenInvalidSecureDeleteModeValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const SecureDeleteMode newValue = (SecureDeleteMode)55;
                Assert.Throws<ArgumentException>(() => dbPragma.SecureDeleteAsync(newValue));
            }
        }

        [Test]
        public async Task SynchronousAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var synchronous = await dbPragma.SynchronousAsync().ConfigureAwait(false);
                var newValue = synchronous == SynchronousLevel.Normal ? SynchronousLevel.Full : SynchronousLevel.Normal;
                await dbPragma.SynchronousAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.SynchronousAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public void SynchronousAsync_GivenInvalidSynchronousLevelValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const SynchronousLevel newValue = (SynchronousLevel)55;
                Assert.Throws<ArgumentException>(() => dbPragma.SynchronousAsync(newValue));
            }
        }

        [Test]
        public async Task TableInfoAsync_WhenTableExists_ReadsValuesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                connection.Execute("create table test_table ( id int primary key, val text )");

                var tableInfo = await dbPragma.TableInfoAsync("test_table").ConfigureAwait(false);
                var tableInfoCount = tableInfo.Count();

                Assert.NotZero(tableInfoCount);
            }
        }

        [Test]
        public void TableInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.Throws<ArgumentNullException>(() => dbPragma.TableInfoAsync(null));
            }
        }

        [Test]
        public void TableInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var name = new Identifier("aksldjaslk", "asjdkas");
                Assert.Throws<ArgumentException>(() => dbPragma.TableInfoAsync(name));
            }
        }

        [Test]
        public async Task TableXInfoAsync_WhenTableExists_ReadsValuesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                connection.Execute("create table test_table ( id int primary key, val text )");

                var tableInfo = await dbPragma.TableXInfoAsync("test_table").ConfigureAwait(false);
                var tableInfoCount = tableInfo.Count();

                Assert.NotZero(tableInfoCount);
            }
        }

        [Test]
        public void TableXInfoAsync_WhenGivenNullLocalName_ThrowsArgumentNullException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                Assert.Throws<ArgumentNullException>(() => dbPragma.TableXInfoAsync(null));
            }
        }

        [Test]
        public void TableXInfoAsync_WhenGivenMismatchingSchemaName_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var name = new Identifier("aksldjaslk", "asjdkas");
                Assert.Throws<ArgumentException>(() => dbPragma.TableXInfoAsync(name));
            }
        }

        [Test]
        public async Task UserVersionAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                var userVersion = await dbPragma.UserVersionAsync().ConfigureAwait(false);
                var newValue = userVersion == 123 ? 456 : 123;
                await dbPragma.UserVersionAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await dbPragma.UserVersionAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public async Task WalCheckpointAsync_WhenInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);
                var results = await dbPragma.WalCheckpointAsync().ConfigureAwait(false);

                Assert.IsNotNull(results);
            }
        }

        [Test]
        public void WalCheckpointAsync_GivenInvalidWalCheckpointModeValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dbPragma = new DatabasePragma(Dialect, connection, MainSchema);

                const WalCheckpointMode newValue = (WalCheckpointMode)55;
                Assert.Throws<ArgumentException>(() => dbPragma.WalCheckpointAsync(newValue));
            }
        }
    }
}
