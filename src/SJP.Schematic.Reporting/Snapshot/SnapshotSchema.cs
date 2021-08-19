using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Snapshot
{
    /// <summary>
    /// Initializes schema in a given database for storing a snapshot of another database's schema definition.
    /// </summary>
    public class SnapshotSchema
    {
        /// <summary>
        /// Initializes an instance of <see cref="SnapshotSchema"/>.
        /// </summary>
        /// <param name="connection">A connection factory for a database that will have snapshot schema created.</param>
        /// <throws cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</throws>
        public SnapshotSchema(IDbConnectionFactory connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// A database connection that is used to store snapshot data.
        /// </summary>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// <para>
        /// Creates schema requires to store a snapshot of another database's schema definition.
        /// </para>
        /// <para>
        /// This is an idempotent operation, subsequent requests will have no effect.
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> whose completion indicates that schema has been initialized.</returns>
        public async Task EnsureSchemaExistsAsync(CancellationToken cancellationToken = default)
        {
            await Connection.ExecuteAsync(CreateIdentifierDefaultsTable, cancellationToken).ConfigureAwait(false);
            await Connection.ExecuteAsync(CreateDatabaseObjectTable, cancellationToken).ConfigureAwait(false);
            await Connection.ExecuteAsync(DatabaseObjectLookupIndex, cancellationToken).ConfigureAwait(false);
            await Connection.ExecuteAsync(CreateDatabaseCommentTable, cancellationToken).ConfigureAwait(false);
            await Connection.ExecuteAsync(DatabaseCommentLookupIndex, cancellationToken).ConfigureAwait(false);
        }

        private const string CreateIdentifierDefaultsTable = @"
CREATE TABLE IF NOT EXISTS database_identifier_defaults (
    defaults_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    server_name TEXT NULL,
    database_name TEXT NULL,
    schema_name TEXT NULL
)";
        private const string CreateDatabaseObjectTable = @"
CREATE TABLE IF NOT EXISTS database_object (
    object_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    object_type TEXT NOT NULL,
    server_name TEXT NULL,
    database_name TEXT NULL,
    schema_name TEXT NOT NULL,
    local_name TEXT NOT NULL,
    definition_json TEXT NOT NULL
)";

        private const string DatabaseObjectLookupIndex = "CREATE INDEX IF NOT EXISTS ix_database_object_object_type_local_schema_database_name ON database_object (object_type, local_name, schema_name, database_name)";

        private const string CreateDatabaseCommentTable = @"
CREATE TABLE IF NOT EXISTS database_comment (
    comment_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    object_type TEXT NOT NULL,
    server_name TEXT NULL,
    database_name TEXT NULL,
    schema_name TEXT NOT NULL,
    local_name TEXT NOT NULL,
    comment_json TEXT NULL
)";

        private const string DatabaseCommentLookupIndex = "CREATE INDEX IF NOT EXISTS ix_database_comment_object_type_local_schema_database_name ON database_comment (object_type, local_name, schema_name, database_name)";
    }
}
