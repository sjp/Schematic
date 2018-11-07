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

        protected IRelationalDatabase BaseDatabase { get; }

        protected IEnumerable<IRelationalDatabase> Databases { get; }

        public string DefaultSchema => BaseDatabase.DefaultSchema;

        public string ServerName => BaseDatabase.ServerName;

        public string DatabaseName => BaseDatabase.DatabaseName;

        public string DatabaseVersion => BaseDatabase.DatabaseVersion;

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsync(tableName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables =>
            Databases
                .SelectMany(d => d.Tables)
                .DistinctBy(t => t.Name)
                .ToList();

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tablesTasks = Databases.Select(d => d.TablesAsync(cancellationToken));
            var tableCollections = await Task.WhenAll(tablesTasks).ConfigureAwait(false);
            var allTableTasks = tableCollections.SelectMany(t => t);
            var allTables = await Task.WhenAll(allTableTasks).ConfigureAwait(false);

            return allTables
                .DistinctBy(t => t.Name)
                .Select(Task.FromResult)
                .ToList();
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Databases
                .Select(d => d.GetTable(tableName))
                .FirstOrDefault(t => t != null);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var tables = Databases.Select(d => d.GetTableAsync(tableName, cancellationToken)).ToArray();
            var tablesTask = await Task.WhenAll(tables).ConfigureAwait(false);
            return Array.Find(tablesTask, t => t != null);
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views =>
            Databases
                .SelectMany(d => d.Views)
                .DistinctBy(t => t.Name)
                .ToList();

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var viewsTasks = Databases.Select(d => d.ViewsAsync(cancellationToken));
            var viewCollections = await Task.WhenAll(viewsTasks).ConfigureAwait(false);
            var allViewTasks = viewCollections.SelectMany(v => v);
            var allViews = await Task.WhenAll(allViewTasks).ConfigureAwait(false);

            return allViews
                .DistinctBy(v => v.Name)
                .Select(Task.FromResult)
                .ToList();
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Databases
                .Select(d => d.GetView(viewName))
                .FirstOrDefault(v => v != null);
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<IRelationalDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var views = Databases.Select(d => d.GetViewAsync(viewName, cancellationToken)).ToArray();
            var viewsTask = await Task.WhenAll(views).ConfigureAwait(false);
            return Array.Find(viewsTask, t => t != null);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsync(sequenceName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences =>
            Databases
                .SelectMany(d => d.Sequences)
                .DistinctBy(t => t.Name)
                .ToList();

        public async Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequencesTasks = Databases.Select(d => d.SequencesAsync(cancellationToken));
            var sequenceCollections = await Task.WhenAll(sequencesTasks).ConfigureAwait(false);
            var allSequenceTasks = sequenceCollections.SelectMany(s => s);
            var allSequences = await Task.WhenAll(allSequenceTasks).ConfigureAwait(false);

            return allSequences
                .DistinctBy(s => s.Name)
                .Select(Task.FromResult)
                .ToList();
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Databases
                .Select(d => d.GetSequence(sequenceName))
                .FirstOrDefault(s => s != null);
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<IDatabaseSequence> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequences = Databases.Select(d => d.GetSequenceAsync(sequenceName, cancellationToken)).ToArray();
            var sequencesTask = await Task.WhenAll(sequences).ConfigureAwait(false);
            return Array.Find(sequencesTask, t => t != null);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsync(synonymName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms =>
            Databases
                .SelectMany(d => d.Synonyms)
                .DistinctBy(t => t.Name)
                .ToList();

        public async Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var synonymsTasks = Databases.Select(d => d.SynonymsAsync(cancellationToken));
            var synonymCollections = await Task.WhenAll(synonymsTasks).ConfigureAwait(false);
            var allSynonymTasks = synonymCollections.SelectMany(s => s);
            var allSynonyms = await Task.WhenAll(allSynonymTasks).ConfigureAwait(false);

            return allSynonyms
                .DistinctBy(s => s.Name)
                .Select(Task.FromResult)
                .ToList();
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Databases
                .Select(d => d.GetSynonym(synonymName))
                .FirstOrDefault(s => s != null);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonyms = Databases.Select(d => d.GetSynonymAsync(synonymName, cancellationToken)).ToArray();
            var synonymsTask = await Task.WhenAll(synonyms).ConfigureAwait(false);
            return Array.Find(synonymsTask, t => t != null);
        }
    }
}
