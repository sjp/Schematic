using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A relational database used to access and manage a PostgreSQL database.
    /// </summary>
    /// <seealso cref="IRelationalDatabase"/>
    public class PostgreSqlRelationalDatabase : IRelationalDatabase
    {
        public PostgreSqlRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

            _tableProvider = new PostgreSqlRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver);
            _viewProvider = new PostgreSqlDatabaseViewProvider(connection, identifierDefaults, identifierResolver);
            _sequenceProvider = new PostgreSqlDatabaseSequenceProvider(connection, identifierDefaults, identifierResolver);
            _routineProvider = new PostgreSqlDatabaseRoutineProvider(connection.DbConnection, identifierDefaults, identifierResolver);
        }

        public IIdentifierDefaults IdentifierDefaults { get; }

        public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
        {
            return _tableProvider.GetAllTables(cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.GetTable(tableName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default)
        {
            return _viewProvider.GetAllViews(cancellationToken);
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return _viewProvider.GetView(viewName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default)
        {
            return _sequenceProvider.GetAllSequences(cancellationToken);
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.GetSequence(sequenceName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
        {
            return SynonymProvider.GetAllSynonyms(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymProvider.GetSynonym(synonymName, cancellationToken);
        }

        public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default)
        {
            return _routineProvider.GetAllRoutines(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return _routineProvider.GetRoutine(routineName, cancellationToken);
        }

        private readonly IRelationalDatabaseTableProvider _tableProvider;
        private readonly IDatabaseViewProvider _viewProvider;
        private readonly IDatabaseSequenceProvider _sequenceProvider;
        private readonly IDatabaseRoutineProvider _routineProvider;
        private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
    }
}