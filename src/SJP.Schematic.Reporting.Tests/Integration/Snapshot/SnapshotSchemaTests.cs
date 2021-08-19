using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Integration.Snapshot
{
    internal sealed class SnapshotSchemaTests
    {
        private TemporaryDirectory _tempDir;
        private IDbConnectionFactory _connectionFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _tempDir = new TemporaryDirectory();

            var dbPath = Path.Combine(_tempDir.DirectoryPath, "snapshot-schema-test.db");

            var builder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            _connectionFactory = new SqliteConnectionFactory(builder.ToString());
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public async Task EnsureSchemaExists_WhenInvoked_CreatesExpectedSchema()
        {
            var dialect = new SqliteDialect();
            var connection = new SchematicConnection(_connectionFactory, dialect);
            var database = await dialect.GetRelationalDatabaseAsync(connection).ConfigureAwait(false);

            var snapshotSchema = new SnapshotSchema(_connectionFactory);
            await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);

            var tables = await database.GetAllTables().ToListAsync().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                var identifierDefaultsTable = tables.Find(t => t.Name.LocalName == "database_identifier_defaults");
                Assert.That(identifierDefaultsTable, Is.Not.Null);

                var databaseObjectsTable = tables.Find(t => t.Name.LocalName == "database_object");
                Assert.That(databaseObjectsTable, Is.Not.Null);

                var objectLookupIndex = databaseObjectsTable.Indexes.FirstOrDefault(i => i.Name.LocalName == "ix_database_object_object_type_local_schema_database_name");
                Assert.That(objectLookupIndex, Is.Not.Null);

                var databaseCommentsTable = tables.Find(t => t.Name.LocalName == "database_comment");
                Assert.That(databaseCommentsTable, Is.Not.Null);

                var commentLookupIndex = databaseCommentsTable.Indexes.FirstOrDefault(i => i.Name.LocalName == "ix_database_comment_object_type_local_schema_database_name");
                Assert.That(commentLookupIndex, Is.Not.Null);
            });
        }

        [Test]
        public void EnsureSchemaExists_WhenInvokedMultipleTimes_IsIdempotent()
        {
            var snapshotSchema = new SnapshotSchema(_connectionFactory);

            Assert.That(async () =>
            {
                await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);
                await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);
            }, Throws.Nothing);
        }
    }
}
