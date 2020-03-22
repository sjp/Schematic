using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    [TestFixture]
    internal sealed class SqliteRelationalDatabaseTests : SqliteTest
    {
        [Test]
        public void VacuumAsync_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = GetSqliteDatabase();
            Assert.That(async () => await sqliteDb.VacuumAsync().ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void VacuumAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = GetSqliteDatabase();
            Assert.That(async () => await sqliteDb.VacuumAsync("main").ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void VacuumAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = GetSqliteDatabase();
            Assert.That(async () => await sqliteDb.VacuumAsync("this_database_should_not_exist").ConfigureAwait(false), Throws.TypeOf<SqliteException>());
        }

        [Test]
        public void AttachDatabaseAsync_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = GetSqliteDatabase();
            Assert.That(async () => await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public async Task DetachDatabaseAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = GetSqliteDatabase();
            await sqliteDb.AttachDatabaseAsync("test_detach", ":memory:").ConfigureAwait(false);
            Assert.That(async () => await sqliteDb.DetachDatabaseAsync("test_detach").ConfigureAwait(false), Throws.Nothing);
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = GetSqliteDatabase();
            Assert.That(async () => await sqliteDb.DetachDatabaseAsync("this_database_should_not_exist").ConfigureAwait(false), Throws.TypeOf<SqliteException>());
        }
    }
}
