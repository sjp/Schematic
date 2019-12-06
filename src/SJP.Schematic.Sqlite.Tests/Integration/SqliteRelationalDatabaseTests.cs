using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    [TestFixture]
    internal sealed class SqliteRelationalDatabaseTests : SqliteTest
    {
        [Test]
        public async Task VacuumAsync_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            await sqliteDb.VacuumAsync().ConfigureAwait(false);
            Assert.Pass();
        }

        [Test]
        public async Task VacuumAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            await sqliteDb.VacuumAsync("main").ConfigureAwait(false);
            Assert.Pass();
        }

        [Test]
        public void VacuumAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
            Assert.ThrowsAsync<SqliteException>(() => sqliteDb.VacuumAsync("test"));
        }

        [Test]
        public async Task AttachDatabaseAsync_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false);
            Assert.Pass();
        }

        [Test]
        public async Task DetachDatabaseAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false);
            await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false);
            Assert.Pass();
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection, IdentifierDefaults);
            Assert.ThrowsAsync<SqliteException>(() => sqliteDb.DetachDatabaseAsync("test"));
        }
    }
}
