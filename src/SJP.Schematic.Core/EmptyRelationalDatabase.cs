using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyRelationalDatabase : IRelationalDatabase
    {
        public EmptyRelationalDatabase(IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        public IDatabaseDialect Dialect { get; }

        public IIdentifierDefaults IdentifierDefaults { get; }

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            return TableProvider.GetAllTables(cancellationToken);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableProvider.GetTable(tableName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ViewProvider.GetAllViews(cancellationToken);
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ViewProvider.GetView(viewName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SequenceProvider.GetAllSequences(cancellationToken);
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return SequenceProvider.GetSequence(sequenceName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken))
        {
            return SynonymProvider.GetAllSynonyms(cancellationToken);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymProvider.GetSynonym(synonymName, cancellationToken);
        }

        public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken))
        {
            return RoutineProvider.GetAllRoutines(cancellationToken);
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return RoutineProvider.GetRoutine(routineName, cancellationToken);
        }

        private static readonly IRelationalDatabaseTableProvider TableProvider = new EmptyRelationalDatabaseTableProvider();
        private static readonly IDatabaseViewProvider ViewProvider = new EmptyDatabaseViewProvider();
        private static readonly IDatabaseSequenceProvider SequenceProvider = new EmptyDatabaseSequenceProvider();
        private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
        private static readonly IDatabaseRoutineProvider RoutineProvider = new EmptyDatabaseRoutineProvider();
    }
}
