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

        protected IRelationalDatabase BaseDatabase { get; }

        protected IEnumerable<IRelationalDatabase> Databases { get; }

        public string DefaultSchema => BaseDatabase.DefaultSchema;

        public string ServerName => BaseDatabase.ServerName;

        public string DatabaseName => BaseDatabase.DatabaseName;

        public string DatabaseVersion => BaseDatabase.DatabaseVersion;

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableSync(tableName);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tablesTasks = Databases.Select(d => d.TablesAsync(cancellationToken));
            var tableCollections = await Task.WhenAll(tablesTasks).ConfigureAwait(false);
            var allTables = tableCollections.SelectMany(t => t);

            return allTables
                .DistinctBy(t => t.Name)
                .ToList();
        }

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Databases
                .Select(d => d.GetTable(tableName))
                .FirstOrDefault(t => t.IsSome);
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var tables = await Databases
                .Select(d => d.GetTableAsync(tableName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.HeadOrNone();
        }

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var viewsTasks = Databases.Select(d => d.ViewsAsync(cancellationToken));
            var viewCollections = await Task.WhenAll(viewsTasks).ConfigureAwait(false);
            var allViews = viewCollections.SelectMany(v => v);

            return allViews
                .DistinctBy(v => v.Name)
                .ToList();
        }

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Databases
                .Select(d => d.GetView(viewName))
                .FirstOrDefault(v => v.IsSome);
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var views = await Databases
                .Select(d => d.GetViewAsync(viewName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.HeadOrNone();
        }

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceSync(sequenceName);
        }

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var sequencesTasks = Databases.Select(d => d.SequencesAsync(cancellationToken));
            var sequenceCollections = await Task.WhenAll(sequencesTasks).ConfigureAwait(false);
            var allSequences = sequenceCollections.SelectMany(s => s);

            return allSequences
                .DistinctBy(v => v.Name)
                .ToList();
        }

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Databases
                .Select(d => d.GetSequence(sequenceName))
                .FirstOrDefault(s => s.IsSome);
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequences = await Databases
                .Select(d => d.GetSequenceAsync(sequenceName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.HeadOrNone();
        }

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymSync(synonymName);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var synonymsTasks = Databases.Select(d => d.SynonymsAsync(cancellationToken));
            var synonymCollections = await Task.WhenAll(synonymsTasks).ConfigureAwait(false);
            var allSynonyms = synonymCollections.SelectMany(s => s);

            return allSynonyms
                .DistinctBy(s => s.Name)
                .ToList();
        }

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Databases
                .Select(d => d.GetSynonym(synonymName))
                .FirstOrDefault(s => s.IsSome);
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonyms = await Databases
                .Select(d => d.GetSynonymAsync(synonymName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.HeadOrNone();
        }
    }
}
