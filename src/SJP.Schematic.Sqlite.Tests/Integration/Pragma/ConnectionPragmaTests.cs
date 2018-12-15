using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Dapper;
using SJP.Schematic.Core;
using System.Linq;
using System.Data;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Sqlite.Pragma;
using Moq;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite.Tests.Integration.Pragma
{
    internal sealed class ConnectionPragmaTests : SqliteTest
    {
        private static IDbConnection CreateConnection()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            return connection;
        }

        [Test]
        public static void DatabasePragmas_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
        {
            var expectedSchemas = new HashSet<string>(new[] { "main", "temp" }, StringComparer.OrdinalIgnoreCase);

            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dbPragmaNames = connPragma.DatabasePragmas.Select(d => d.SchemaName).ToList();
                var onlyExpectedPresent = dbPragmaNames.All(name => expectedSchemas.Contains(name));

                Assert.IsTrue(onlyExpectedPresent);
            }
        }

        [Test]
        public static async Task DatabasePragmasAsync_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
        {
            var expectedSchemas = new HashSet<string>(new[] { "main", "temp" }, StringComparer.OrdinalIgnoreCase);

            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dbPragmas = await connPragma.DatabasePragmasAsync().ConfigureAwait(false);
                var dbPragmaNames = dbPragmas.Select(d => d.SchemaName).ToList();
                var onlyExpectedPresent = dbPragmaNames.All(name => expectedSchemas.Contains(name));

                Assert.IsTrue(onlyExpectedPresent);
            }
        }

        [Test]
        public static void DatabaseList_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
        {
            var expectedSchemas = new HashSet<string>(new[] { "main", "temp" }, StringComparer.OrdinalIgnoreCase);

            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dbNames = connPragma.DatabaseList.Select(d => d.name).ToList();
                var onlyExpectedPresent = dbNames.All(name => expectedSchemas.Contains(name));

                Assert.IsTrue(onlyExpectedPresent);
            }
        }

        [Test]
        public static async Task DatabaseListAsync_ForNewDatabase_ShouldBeMainAndTempSchemasOnly()
        {
            var expectedSchemas = new HashSet<string>(new[] { "main", "temp" }, StringComparer.OrdinalIgnoreCase);

            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dbLists = await connPragma.DatabaseListAsync().ConfigureAwait(false);
                var dbNames = dbLists.Select(d => d.name).ToList();
                var onlyExpectedPresent = dbNames.All(name => expectedSchemas.Contains(name));

                Assert.IsTrue(onlyExpectedPresent);
            }
        }

        [Test]
        public static void AutomaticIndex_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var automaticIndex = connPragma.AutomaticIndex;
                var newValue = !automaticIndex;
                connPragma.AutomaticIndex = newValue;
                var readOfNewValue = connPragma.AutomaticIndex;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task AutomaticIndexAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var automaticIndex = await connPragma.AutomaticIndexAsync().ConfigureAwait(false);
                var newValue = !automaticIndex;
                await connPragma.AutomaticIndexAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.AutomaticIndexAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void BusyTimeout_PropertyGet_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var busyTimeout = connPragma.BusyTimeout;
                var defaultValue = new TimeSpan(0, 0, 0);

                Assert.AreEqual(defaultValue, busyTimeout);
            }
        }

        [Test]
        public static async Task BusyTimeoutAsync_Get_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var busyTimeout = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);
                var defaultValue = new TimeSpan(0, 0, 0);

                Assert.AreEqual(defaultValue, busyTimeout);
            }
        }

        [Test]
        [Ignore("SqliteConnection does not support setting of busy_timeout pragmas yet.")]
        public static void BusyTimeout_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var busyTimeout = connPragma.BusyTimeout;
                var newValue = new TimeSpan(0, 0, 23);
                connPragma.BusyTimeout = newValue;
                var readOfNewValue = connPragma.BusyTimeout;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        [Ignore("SqliteConnection does not support setting of busy_timeout pragmas yet.")]
        public static async Task BusyTimeoutAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var busyTimeout = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);
                var newValue = new TimeSpan(0, 0, 23);
                await connPragma.BusyTimeoutAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.BusyTimeoutAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void CaseSensitiveLike_PropertySet_WritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                connection.Execute("create table test ( col text )");
                connection.Execute("insert into test (col) values ('asdf')");
                connection.Execute("insert into test (col) values ('ASDASDASD')");
                connection.Execute("insert into test (col) values ('ASDASDA')");

                const string query = "select count(*) from test where col like 'ASD%'";
                const int expectedInsensitive = 3;
                const int expectedSensitive = 2;

                connPragma.CaseSensitiveLike(false);
                var insensitiveResult = connection.ExecuteScalar<int>(query);

                connPragma.CaseSensitiveLike(true);
                var sensitiveResult = connection.ExecuteScalar<int>(query);

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(expectedInsensitive, insensitiveResult);
                    Assert.AreEqual(expectedSensitive, sensitiveResult);
                });
            }
        }

        [Test]
        public static async Task CaseSensitiveLikeAsync_WhenSet_WritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                await connection.ExecuteAsync("create table test ( col text )").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test (col) values ('asdf')").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test (col) values ('ASDASDASD')").ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test (col) values ('ASDASDA')").ConfigureAwait(false);

                const string query = "select count(*) from test where col like 'ASD%'";
                const int expectedInsensitive = 3;
                const int expectedSensitive = 2;

                await connPragma.CaseSensitiveLikeAsync(false).ConfigureAwait(false);
                var insensitiveResult = connection.ExecuteScalar<int>(query);

                await connPragma.CaseSensitiveLikeAsync(true).ConfigureAwait(false);
                var sensitiveResult = connection.ExecuteScalar<int>(query);

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(expectedInsensitive, insensitiveResult);
                    Assert.AreEqual(expectedSensitive, sensitiveResult);
                });
            }
        }

        [Test]
        public static void CellSizeCheck_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var cellSizeCheck = connPragma.CellSizeCheck;
                var newValue = !cellSizeCheck;
                connPragma.CellSizeCheck = newValue;
                var readOfNewValue = connPragma.CellSizeCheck;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task CellSizeCheckAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var cellSizeCheck = await connPragma.CellSizeCheckAsync().ConfigureAwait(false);
                var newValue = !cellSizeCheck;
                await connPragma.CellSizeCheckAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.CellSizeCheckAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void CheckpointFullFsync_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var checkpointFullFsync = connPragma.CheckpointFullFsync;
                var newValue = !checkpointFullFsync;
                connPragma.CheckpointFullFsync = newValue;
                var readOfNewValue = connPragma.CheckpointFullFsync;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task CheckpointFullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var checkpointFullFsync = await connPragma.CheckpointFullFsyncAsync().ConfigureAwait(false);
                var newValue = !checkpointFullFsync;
                await connPragma.CheckpointFullFsyncAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.CheckpointFullFsyncAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void CollationList_PropertyGet_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var collations = connPragma.CollationList.ToList();

                Assert.NotZero(collations.Count);
            }
        }

        [Test]
        public static async Task CollationListAsync_GetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var collations = await connPragma.CollationListAsync().ConfigureAwait(false);
                var collationList = collations.ToList();

                Assert.NotZero(collationList.Count);
            }
        }

        [Test]
        public static void CompileOptions_PropertyGet_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var options = connPragma.CompileOptions.ToList();

                Assert.NotZero(options.Count);
            }
        }

        [Test]
        public static async Task CompileOptionsAsync_GetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var options = await connPragma.CompileOptionsAsync().ConfigureAwait(false);
                var optionsList = options.ToList();

                Assert.NotZero(optionsList.Count);
            }
        }

        [Test]
        public static void DataVersion_PropertyGet_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dataVersion = connPragma.DataVersion;

                Assert.NotZero(dataVersion);
            }
        }

        [Test]
        public static async Task DataVersionAsync_GetInvoked_ReadsCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var dataVersion = await connPragma.DataVersionAsync().ConfigureAwait(false);

                Assert.NotZero(dataVersion);
            }
        }

        [Test]
        public static void DeferForeignKeys_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var deferForeignKeys = connPragma.DeferForeignKeys;
                var newValue = !deferForeignKeys;
                connPragma.DeferForeignKeys = newValue;
                var readOfNewValue = connPragma.DeferForeignKeys;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task DeferForeignKeysAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var deferForeignKeys = await connPragma.DeferForeignKeysAsync().ConfigureAwait(false);
                var newValue = !deferForeignKeys;
                await connPragma.DeferForeignKeysAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.DeferForeignKeysAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void Encoding_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var textEncoding = connPragma.Encoding;
                var newValue = textEncoding == Encoding.Utf8 ? Encoding.Utf16le : Encoding.Utf8;
                connPragma.Encoding = newValue;
                var readOfNewValue = connPragma.Encoding;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void Encoding_GivenInvalidEncodingValue_ThrowsArugmentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const Encoding newValue = (Encoding)55;
                Assert.Throws<ArgumentException>(() => connPragma.Encoding = newValue);
            }
        }

        [Test]
        public static async Task EncodingAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var textEncoding = await connPragma.EncodingAsync().ConfigureAwait(false);
                var newValue = textEncoding == Encoding.Utf8 ? Encoding.Utf16le : Encoding.Utf8;
                await connPragma.EncodingAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.EncodingAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void EncodingAsync_GivenInvalidEncodingValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const Encoding newValue = (Encoding)55;
                Assert.Throws<ArgumentException>(() => connPragma.EncodingAsync(newValue));
            }
        }

        [Test]
        public static void ForeignKeys_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var foreignKeys = connPragma.ForeignKeys;
                var newValue = !foreignKeys;
                connPragma.ForeignKeys = newValue;
                var readOfNewValue = connPragma.ForeignKeys;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task ForeignKeysAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var foreignKeys = await connPragma.ForeignKeysAsync().ConfigureAwait(false);
                var newValue = !foreignKeys;
                await connPragma.ForeignKeysAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.ForeignKeysAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void FullFsync_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var fullFsync = connPragma.FullFsync;
                var newValue = !fullFsync;
                connPragma.FullFsync = newValue;
                var readOfNewValue = connPragma.FullFsync;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task FullFsyncAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var fullFsync = await connPragma.FullFsyncAsync().ConfigureAwait(false);
                var newValue = !fullFsync;
                await connPragma.FullFsyncAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.FullFsyncAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void IgnoreCheckConstraints_PropertySet_WritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                connection.Execute("create table test ( col text, constraint col_ck check (col <> 'asdf') )");

                connPragma.IgnoreCheckConstraints(true);
                connection.Execute("insert into test (col) values ('asdf')");

                connPragma.IgnoreCheckConstraints(false);
                Assert.Throws<SqliteException>(() => connection.Execute("insert into test (col) values ('asdf')"));
            }
        }

        [Test]
        public static async Task IgnoreCheckConstraintsAsync_WhenSet_WritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                await connection.ExecuteAsync("create table test ( col text, constraint col_ck check (col <> 'asdf') )").ConfigureAwait(false);

                await connPragma.IgnoreCheckConstraintsAsync(true).ConfigureAwait(false);
                await connection.ExecuteAsync("insert into test (col) values ('asdf')").ConfigureAwait(false);

                await connPragma.IgnoreCheckConstraintsAsync(false).ConfigureAwait(false);
                Assert.ThrowsAsync<SqliteException>(async () => await connection.ExecuteAsync("insert into test (col) values ('asdf')").ConfigureAwait(false));
            }
        }

        [Test]
        [Ignore("Disabled until SQLite driver has been updated to v3.26.0.")]
        public static void LegacyAlterTable_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var legacyAlterTable = connPragma.LegacyAlterTable;
                var newValue = !legacyAlterTable;
                connPragma.LegacyAlterTable = newValue;
                var readOfNewValue = connPragma.LegacyAlterTable;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        [Ignore("Disabled until SQLite driver has been updated to v3.26.0.")]
        public static async Task LegacyAlterTableAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var legacyAlterTable = await connPragma.LegacyAlterTableAsync().ConfigureAwait(false);
                var newValue = !legacyAlterTable;
                await connPragma.LegacyAlterTableAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.LegacyAlterTableAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void LegacyFileFormat_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var legacyFileFormat = connPragma.LegacyFileFormat;
                var newValue = !legacyFileFormat;
                connPragma.LegacyFileFormat = newValue;
                var readOfNewValue = connPragma.LegacyFileFormat;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task LegacyFileFormatAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var legacyFileFormat = await connPragma.LegacyFileFormatAsync().ConfigureAwait(false);
                var newValue = !legacyFileFormat;
                await connPragma.LegacyFileFormatAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.LegacyFileFormatAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void Optimize_WhenInvoked_PerformsOperationSuccessfully()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                connPragma.Optimize();
                Assert.Pass();
            }
        }

        [Test]
        public static void Optimize_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const OptimizeFeatures newValue = (OptimizeFeatures)55;
                Assert.Throws<ArgumentException>(() => connPragma.Optimize(newValue));
            }
        }

        [Test]
        public static async Task OptimizeAsync_WhenInvoked_PerformsOperationSuccessfully()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                await connPragma.OptimizeAsync().ConfigureAwait(false);
                Assert.Pass();
            }
        }

        [Test]
        public static void OptimizeAsync_GivenInvalidOptimizeFeaturesValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const OptimizeFeatures newValue = (OptimizeFeatures)55;
                Assert.Throws<ArgumentException>(() => connPragma.OptimizeAsync(newValue));
            }
        }

        [Test]
        public static void QueryOnly_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var queryOnly = connPragma.QueryOnly;
                var newValue = !queryOnly;
                connPragma.QueryOnly = newValue;
                var readOfNewValue = connPragma.QueryOnly;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task QueryOnlyAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var queryOnly = await connPragma.QueryOnlyAsync().ConfigureAwait(false);
                var newValue = !queryOnly;
                await connPragma.QueryOnlyAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.QueryOnlyAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void ReadUncommitted_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var readUncommitted = connPragma.ReadUncommitted;
                var newValue = !readUncommitted;
                connPragma.ReadUncommitted = newValue;
                var readOfNewValue = connPragma.ReadUncommitted;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task ReadUncommittedAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var readUncommitted = await connPragma.ReadUncommittedAsync().ConfigureAwait(false);
                var newValue = !readUncommitted;
                await connPragma.ReadUncommittedAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.ReadUncommittedAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void RecursiveTriggers_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var recursiveTriggers = connPragma.RecursiveTriggers;
                var newValue = !recursiveTriggers;
                connPragma.RecursiveTriggers = newValue;
                var readOfNewValue = connPragma.RecursiveTriggers;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task RecursiveTriggersAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var recursiveTriggers = await connPragma.RecursiveTriggersAsync().ConfigureAwait(false);
                var newValue = !recursiveTriggers;
                await connPragma.RecursiveTriggersAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.RecursiveTriggersAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void ReverseUnorderedSelects_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var recursiveTriggers = connPragma.ReverseUnorderedSelects;
                var newValue = !recursiveTriggers;
                connPragma.ReverseUnorderedSelects = newValue;
                var readOfNewValue = connPragma.ReverseUnorderedSelects;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task ReverseUnorderedSelectsAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var recursiveTriggers = await connPragma.ReverseUnorderedSelectsAsync().ConfigureAwait(false);
                var newValue = !recursiveTriggers;
                await connPragma.ReverseUnorderedSelectsAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.ReverseUnorderedSelectsAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void ShrinkMemory_WhenInvoked_PerformsOperationSuccessfully()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                connPragma.ShrinkMemory();
                Assert.Pass();
            }
        }

        [Test]
        public static async Task ShrinkMemoryAsync_WhenInvoked_PerformsOperationSuccessfully()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                await connPragma.ShrinkMemoryAsync().ConfigureAwait(false);
                Assert.Pass();
            }
        }

        [Test]
        public static void SoftHeapLimit_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var softHeapLimit = connPragma.SoftHeapLimit;
                var newValue = softHeapLimit == 0 ? 9000 : 0;
                connPragma.SoftHeapLimit = newValue;
                var readOfNewValue = connPragma.SoftHeapLimit;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task SoftHeapLimitAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var softHeapLimit = await connPragma.SoftHeapLimitAsync().ConfigureAwait(false);
                var newValue = softHeapLimit == 0 ? 9000 : 0;
                await connPragma.SoftHeapLimitAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.SoftHeapLimitAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void TemporaryStore_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var tempStore = connPragma.TemporaryStore;
                var newValue = tempStore == TemporaryStoreLocation.Default ? TemporaryStoreLocation.File : TemporaryStoreLocation.Default;
                connPragma.TemporaryStore = newValue;
                var readOfNewValue = connPragma.TemporaryStore;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void TemporaryStore_GivenInvalidTemporaryStoreLocationValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const TemporaryStoreLocation tempStore = (TemporaryStoreLocation)55;
                Assert.Throws<ArgumentException>(() => connPragma.TemporaryStore = tempStore);
            }
        }

        [Test]
        public static async Task TemporaryStoreAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var tempStore = await connPragma.TemporaryStoreAsync().ConfigureAwait(false);
                var newValue = tempStore == TemporaryStoreLocation.Default ? TemporaryStoreLocation.File : TemporaryStoreLocation.Default;
                await connPragma.TemporaryStoreAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.TemporaryStoreAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void TemporaryStoreAsync_GivenInvalidTemporaryStoreLocationValue_ThrowsArgumentException()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                const TemporaryStoreLocation tempStore = (TemporaryStoreLocation)55;
                Assert.Throws<ArgumentException>(() => connPragma.TemporaryStoreAsync(tempStore));
            }
        }

        [Test]
        public static void Threads_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var threads = connPragma.Threads;
                var newValue = threads == 0 ? 8 : 0;
                connPragma.Threads = newValue;
                var readOfNewValue = connPragma.Threads;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task ThreadsAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var threads = await connPragma.ThreadsAsync().ConfigureAwait(false);
                var newValue = threads == 0 ? 8 : 0;
                await connPragma.ThreadsAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.ThreadsAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void WalAutoCheckpoint_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var autoCheckpoint = connPragma.WalAutoCheckpoint;
                var newValue = autoCheckpoint == 50 ? 100 : 50;
                connPragma.WalAutoCheckpoint = newValue;
                var readOfNewValue = connPragma.WalAutoCheckpoint;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task WalAutoCheckpointAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var autoCheckpoint = await connPragma.WalAutoCheckpointAsync().ConfigureAwait(false);
                var newValue = autoCheckpoint == 50 ? 100 : 50;
                await connPragma.WalAutoCheckpointAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.WalAutoCheckpointAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static void WritableSchema_PropertyGetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var writableSchema = connPragma.WritableSchema;
                var newValue = !writableSchema;
                connPragma.WritableSchema = newValue;
                var readOfNewValue = connPragma.WritableSchema;

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }

        [Test]
        public static async Task WritableSchemaAsync_GetAndSet_ReadsAndWritesCorrectly()
        {
            using (var connection = CreateConnection())
            {
                var dialect = Mock.Of<IDatabaseDialect>();
                var connPragma = new ConnectionPragma(dialect, connection);

                var writableSchema = await connPragma.WritableSchemaAsync().ConfigureAwait(false);
                var newValue = !writableSchema;
                await connPragma.WritableSchemaAsync(newValue).ConfigureAwait(false);
                var readOfNewValue = await connPragma.WritableSchemaAsync().ConfigureAwait(false);

                Assert.AreEqual(newValue, readOfNewValue);
            }
        }
    }
}
