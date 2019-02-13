using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Sqlite
{
    /// <summary>
    /// A relational database used to access and manage a SQLite database.
    /// </summary>
    public class SqliteRelationalDatabase : RelationalDatabase, ISqliteDatabase
    {
        /// <summary>
        /// Constructs a new <see cref="SqliteRelationalDatabase"/>.
        /// </summary>
        /// <param name="dialect">A database dialect for SQLite.</param>
        /// <param name="connection">The connection to a SQLite database.</param>
        /// <param name="identifierDefaults">Default values for identifier components.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dialect"/>, <paramref name="connection"/>, or <paramref name="identifierDefaults"/> are <code>null</code>.</exception>
        public SqliteRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
            var pragma = new ConnectionPragma(Dialect, Connection);
            _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
            _viewProvider = new SqliteDatabaseViewProvider(connection, pragma, dialect, identifierDefaults, dialect.TypeProvider);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _tableProvider.GetAllTables(cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTable(tableName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _viewProvider.GetAllViews(cancellationToken);
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetView(viewName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _sequenceProvider.GetAllSequences(cancellationToken);
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequence(sequenceName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _synonymProvider.GetAllSynonyms(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymProvider.GetSynonym(synonymName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _routineProvider.GetAllRoutines(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineProvider.GetRoutine(routineName, cancellationToken);
        }

        /// <summary>
        /// Adds another database file to the current database connection.
        /// </summary>
        /// <param name="schemaName">The name to assign for the attached database.</param>
        /// <param name="fileName">The path to a SQLite database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="fileName"/> or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <code>main</code>.</exception>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));
            if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("'main' is not a valid name to assign to an attached database. It will always be present.", nameof(schemaName));

            var sql = AttachDatabaseQuery(schemaName, fileName);
            return Connection.ExecuteAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Constructs a SQL query that adds another database file to the current database connection.
        /// </summary>
        /// <param name="schemaName">The name to assign for the attached database.</param>
        /// <param name="fileName">The path to a SQLite database.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="fileName"/> or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <code>main</code>.</exception>
        /// <returns>A SQL query that can be used to add a database file to the current connection.</returns>
        protected virtual string AttachDatabaseQuery(string schemaName, string fileName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));
            if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("'main' is not a valid name to assign to an attached database. It will always be present.", nameof(schemaName));

            var quotedSchemaName = Dialect.QuoteIdentifier(schemaName);
            var escapedFileName = fileName.Replace("'", "''");

            return $"ATTACH DATABASE '{ escapedFileName }' AS { quotedSchemaName }";
        }

        /// <summary>
        /// Removes a database file from the current database connection.
        /// </summary>
        /// <param name="schemaName">The name of an attached database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <code>main</code>.</exception>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("'main' is not a valid database name to remove. It must always be present.", nameof(schemaName));

            var sql = DetachDatabaseQuery(schemaName);
            return Connection.ExecuteAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Constructs a SQL query that removes an attached database from the current database connection.
        /// </summary>
        /// <param name="schemaName">The name of an attached database.</param>
        /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <code>main</code>.</exception>
        /// <returns>A SQL query that can be used to remove a database file from the current connection.</returns>
        protected virtual string DetachDatabaseQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("'main' is not a valid database name to remove. It must always be present.", nameof(schemaName));

            return "DETACH DATABASE " + Dialect.QuoteIdentifier(schemaName);
        }

        /// <summary>
        /// The <code>VACUUM</code> command rebuilds the database file, repacking it into a minimal amount of disk space.
        /// </summary>
        /// <remarks>This will be run only on the main database.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task VacuumAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "VACUUM";
            return Connection.ExecuteAsync(sql, cancellationToken);
        }

        /// <summary>
        /// The <code>VACUUM</code> command rebuilds the database file, repacking it into a minimal amount of disk space.
        /// </summary>
        /// <param name="schemaName">The name of an attached database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            var sql = VacuumQuery(schemaName);
            return Connection.ExecuteAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Constructs a SQL query that rebuild and repack a database file.
        /// </summary>
        /// <param name="schemaName">The name of an attached database.</param>
        /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
        /// <returns>A SQL query that can be used to rebuild and repack a database file.</returns>
        protected virtual string VacuumQuery(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            return "VACUUM " + Dialect.QuoteIdentifier(schemaName);
        }

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IDatabaseViewProvider _viewProvider;
        private static readonly IDatabaseSequenceProvider _sequenceProvider = new EmptyDatabaseSequenceProvider();
        private static readonly IDatabaseSynonymProvider _synonymProvider = new EmptyDatabaseSynonymProvider();
        private static readonly IDatabaseRoutineProvider _routineProvider = new EmptyDatabaseRoutineProvider();
    }
}
