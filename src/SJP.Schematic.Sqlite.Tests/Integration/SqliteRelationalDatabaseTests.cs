using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal sealed class SqliteRelationalDatabaseTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

        [Test]
        public void ServerName_PropertyGet_ShouldBeNull()
        {
            Assert.IsNull(Database.ServerName);
        }

        [Test]
        public void Database_PropertyGet_ShouldBeNull()
        {
            Assert.IsNull(Database.DatabaseName);
        }

        [Test]
        public void DefaultSchema_PropertyGetForDefaultCtor_ShouldEqualMain()
        {
            Assert.AreEqual("main", Database.DefaultSchema);
        }

        [Test]
        public void DefaultSchema_PropertyGetWhenGivenNameInCtor_ShouldEqualCtorArg()
        {
            const string defaultSchema = "test_schema";
            var database = new SqliteRelationalDatabase(Dialect, Connection, defaultSchema);

            Assert.AreEqual(defaultSchema, database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.NotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }

        [Test]
        public void Vacuum_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            sqliteDb.Vacuum();
        }

        [Test]
        public Task VacuumAsync_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            return sqliteDb.VacuumAsync();
        }

        [Test]
        public void Vacuum_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            sqliteDb.Vacuum("main");
        }

        [Test]
        public void Vacuum_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            Assert.Throws<SqliteException>(() => sqliteDb.Vacuum("asdas"));
        }

        [Test]
        public Task VacuumAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            return sqliteDb.VacuumAsync("main");
        }

        [Test]
        public void VacuumAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.VacuumAsync("asdas").ConfigureAwait(false));
        }

        [Test]
        public void AttachDatabase_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            sqliteDb.AttachDatabase("test", ":memory:");
        }

        [Test]
        public Task AttachDatabaseAsync_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            return sqliteDb.AttachDatabaseAsync("test", ":memory:");
        }

        [Test]
        public void DetachDatabase_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            sqliteDb.AttachDatabase("test", ":memory:");
            sqliteDb.DetachDatabase("test");
        }

        [Test]
        public async Task DetachDatabaseAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false);
            await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false);
        }

        [Test]
        public void DetachDatabase_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            Assert.Throws<SqliteException>(() => sqliteDb.DetachDatabase("test"));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false));
        }

        internal sealed class SequenceTests : SqliteTest
        {
            private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("asd");
                var sequence = Database.GetSequence(sequenceName);

                Assert.IsTrue(sequence.IsNone);
            }

            [Test]
            public void Sequences_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = Database.Sequences.ToList();
                var count = sequences.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSequenceAsync_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public async Task GetSequenceAsync_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("asd");
                var sequenceIsNone = await Database.GetSequenceAsync(sequenceName).IsNone.ConfigureAwait(false);

                Assert.IsTrue(sequenceIsNone);
            }

            [Test]
            public async Task SequencesAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);

                Assert.Zero(sequences.Count);
            }
        }

        internal sealed class SynonymTests : SqliteTest
        {
            private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("asd");
                var synonym = Database.GetSynonym(synonymName);

                Assert.IsTrue(synonym.IsNone);
            }

            [Test]
            public void Synonyms_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = Database.Synonyms.ToList();
                var count = synonyms.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSynonymAsync_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("asd");
                var synonymIsNone = await Database.GetSynonymAsync(synonymName).IsNone.ConfigureAwait(false);

                Assert.IsTrue(synonymIsNone);
            }

            [Test]
            public async Task SynonymsAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.Zero(synonyms.Count);
            }
        }
    }
}
