using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Modelled
{
    public class OrderedRelationalDatabase : IRelationalDatabase
    {
        // databases in order of preference, most preferred first
        public OrderedRelationalDatabase(IEnumerable<IRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be present in the collection of databases", nameof(databases));

            Databases = databases.ToList();
            BaseDatabase = Databases.Last();
        }

        public IDatabaseDialect Dialect => BaseDatabase.Dialect;

        public IIdentifierDefaults IdentifierDefaults => BaseDatabase.IdentifierDefaults;

        protected IRelationalDatabase BaseDatabase { get; }

        protected IEnumerable<IRelationalDatabase> Databases { get; }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTable(tableName, cancellationToken);
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
        {
            var tablesTasks = Databases.Select(d => d.GetAllTables(cancellationToken));
            var tableCollections = await Task.WhenAll(tablesTasks).ConfigureAwait(false);
            var allTables = tableCollections.SelectMany(t => t);

            return allTables
                .DistinctBy(t => t.Name)
                .ToList();
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var tables = await Databases
                .Select(d => d.GetTable(tableName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.HeadOrNone();
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadView(viewName, cancellationToken);
        }

        public async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
        {
            var viewsTasks = Databases.Select(d => d.GetAllViews(cancellationToken));
            var viewCollections = await Task.WhenAll(viewsTasks).ConfigureAwait(false);
            var allViews = viewCollections.SelectMany(v => v);

            return allViews
                .DistinctBy(v => v.Name)
                .ToList();
        }

        protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var views = await Databases
                .Select(d => d.GetView(viewName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.HeadOrNone();
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequence(sequenceName, cancellationToken);
        }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default)
        {
            var sequencesTasks = Databases.Select(d => d.GetAllSequences(cancellationToken));
            var sequenceCollections = await Task.WhenAll(sequencesTasks).ConfigureAwait(false);
            var allSequences = sequenceCollections.SelectMany(s => s);

            return allSequences
                .DistinctBy(v => v.Name)
                .ToList();
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequences = await Databases
                .Select(d => d.GetSequence(sequenceName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.HeadOrNone();
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonym(synonymName, cancellationToken);
        }

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default)
        {
            var synonymsTasks = Databases.Select(d => d.GetAllSynonyms(cancellationToken));
            var synonymCollections = await Task.WhenAll(synonymsTasks).ConfigureAwait(false);
            var allSynonyms = synonymCollections.SelectMany(s => s);

            return allSynonyms
                .DistinctBy(s => s.Name)
                .ToList();
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonyms = await Databases
                .Select(d => d.GetSynonym(synonymName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.HeadOrNone();
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return LoadRoutine(routineName, cancellationToken);
        }

        public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
        {
            var routinesTasks = Databases.Select(d => d.GetAllRoutines(cancellationToken));
            var routineCollections = await Task.WhenAll(routinesTasks).ConfigureAwait(false);
            var allRoutines = routineCollections.SelectMany(s => s);

            return allRoutines
                .DistinctBy(r => r.Name)
                .ToList();
        }

        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return LoadRoutineAsyncCore(routineName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseRoutine>> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var routines = await Databases
                .Select(d => d.GetRoutine(routineName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return routines.HeadOrNone();
        }
    }
}
