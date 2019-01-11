using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal sealed class SqliteRelationalDatabaseTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

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
        public void DefaultSchema_PropertyGet_ShouldEqualMain()
        {
            Assert.AreEqual("main", Database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.IsNotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }

        [Test]
        public void Vacuum_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            sqliteDb.Vacuum();
        }

        [Test]
        public Task VacuumAsync_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            return sqliteDb.VacuumAsync();
        }

        [Test]
        public void Vacuum_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            sqliteDb.Vacuum("main");
        }

        [Test]
        public void Vacuum_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            Assert.Throws<SqliteException>(() => sqliteDb.Vacuum("asdas"));
        }

        [Test]
        public Task VacuumAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            return sqliteDb.VacuumAsync("main");
        }

        [Test]
        public void VacuumAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.VacuumAsync("asdas").ConfigureAwait(false));
        }

        [Test]
        public void AttachDatabase_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            sqliteDb.AttachDatabase("test", ":memory:");
        }

        [Test]
        public Task AttachDatabaseAsync_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            return sqliteDb.AttachDatabaseAsync("test", ":memory:");
        }

        [Test]
        public void DetachDatabase_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            sqliteDb.AttachDatabase("test", ":memory:");
            sqliteDb.DetachDatabase("test");
        }

        [Test]
        public async Task DetachDatabaseAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false);
            await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false);
        }

        [Test]
        public void DetachDatabase_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            Assert.Throws<SqliteException>(() => sqliteDb.DetachDatabase("test"));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false));
        }
    }
}
