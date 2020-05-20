using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A relational database used to access and manage an Oracle database.
    /// </summary>
    /// <seealso cref="IRelationalDatabase"/>
    public class OracleRelationalDatabase : IRelationalDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleRelationalDatabase"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

            _tableProvider = new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver);
            _viewProvider = new OracleDatabaseViewProvider(connection, identifierDefaults, identifierResolver);
            _sequenceProvider = new OracleDatabaseSequenceProvider(connection.DbConnection, identifierDefaults, identifierResolver);
            _synonymProvider = new OracleDatabaseSynonymProvider(connection.DbConnection, identifierDefaults, identifierResolver);
            _routineProvider = new OracleDatabaseRoutineProvider(connection.DbConnection, identifierDefaults, identifierResolver);
        }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        public IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
        {
            return _tableProvider.GetAllTables(cancellationToken);
        }

        /// <summary>
        /// Gets a database table.
        /// </summary>
        /// <param name="tableName">A database table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTable(tableName, cancellationToken);
        }

        /// <summary>
        /// Gets all database views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default)
        {
            return _viewProvider.GetAllViews(cancellationToken);
        }

        /// <summary>
        /// Gets a database view.
        /// </summary>
        /// <param name="viewName">A database view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetView(viewName, cancellationToken);
        }

        /// <summary>
        /// Gets all database sequences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database sequences.</returns>
        public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default)
        {
            return _sequenceProvider.GetAllSequences(cancellationToken);
        }

        /// <summary>
        /// Gets a database sequence.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequence(sequenceName, cancellationToken);
        }

        /// <summary>
        /// Gets all database synonyms.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database synonyms.</returns>
        public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
        {
            return _synonymProvider.GetAllSynonyms(cancellationToken);
        }

        /// <summary>
        /// Gets a database synonym.
        /// </summary>
        /// <param name="synonymName">A database synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _synonymProvider.GetSynonym(synonymName, cancellationToken);
        }

        /// <summary>
        /// Gets all database routines.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database routines.</returns>
        public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default)
        {
            return _routineProvider.GetAllRoutines(cancellationToken);
        }

        /// <summary>
        /// Gets a database routine.
        /// </summary>
        /// <param name="routineName">A database routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineProvider.GetRoutine(routineName, cancellationToken);
        }

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IDatabaseViewProvider _viewProvider;
        private readonly IDatabaseSequenceProvider _sequenceProvider;
        private readonly IDatabaseSynonymProvider _synonymProvider;
        private readonly IDatabaseRoutineProvider _routineProvider;
    }
}
