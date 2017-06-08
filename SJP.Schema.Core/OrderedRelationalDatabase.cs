using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.Core
{
    public class OrderedRelationalDatabase : IRelationalDatabase
    {
        // databases in order of preference, most preferred first
        public OrderedRelationalDatabase(IEnumerable<IDependentRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be present in the collection of databases", nameof(databases));

            Databases = databases.Select(d => SetParent(this, d)).ToImmutableList();
            BaseDatabase = Databases.Last();

            _tableCache = new AsyncCache<Identifier, IRelationalDatabaseTable>(LoadTableAsync);
            _viewCache = new AsyncCache<Identifier, IRelationalDatabaseView>(LoadViewAsync);
            _sequenceCache = new AsyncCache<Identifier, IDatabaseSequence>(LoadSequenceAsync);
            _synonymCache = new AsyncCache<Identifier, IDatabaseSynonym>(LoadSynonymAsync);
            _triggerCache = new AsyncCache<Identifier, IDatabaseTrigger>(LoadTriggerAsync);

            Table = new LazyDictionaryCache<Identifier, IRelationalDatabaseTable>(tableName => TableAsync(tableName).Result);
            View = new LazyDictionaryCache<Identifier, IRelationalDatabaseView>(viewName => ViewAsync(viewName).Result);
            Sequence = new LazyDictionaryCache<Identifier, IDatabaseSequence>(sequenceName => SequenceAsync(sequenceName).Result);
            Synonym = new LazyDictionaryCache<Identifier, IDatabaseSynonym>(synonymName => SynonymAsync(synonymName).Result);
            Trigger = new LazyDictionaryCache<Identifier, IDatabaseTrigger>(triggerName => TriggerAsync(triggerName).Result);
        }

        public IDatabaseDialect Dialect => BaseDatabase.Dialect;

        protected IRelationalDatabase BaseDatabase { get; }

        protected IEnumerable<IRelationalDatabase> Databases { get; }

        public string DefaultSchema => BaseDatabase.DefaultSchema;

        public string DatabaseName => BaseDatabase.DatabaseName;

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table { get; }

        public IEnumerable<IRelationalDatabaseTable> Tables => TablesAsync().ToList().Wait();

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View { get; }

        public IEnumerable<IRelationalDatabaseView> Views => ViewsAsync().ToList().Wait();

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence { get; }

        public IEnumerable<IDatabaseSequence> Sequences => SequencesAsync().ToList().Wait();

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym { get; }

        public IEnumerable<IDatabaseSynonym> Synonyms => SynonymsAsync().ToList().Wait();

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IEnumerable<IDatabaseTrigger> Triggers => TriggersAsync().ToList().Wait();

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Databases.Any(d => d.TableExists(tableName));
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Databases.Any(d => d.ViewExists(viewName));
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Databases.Any(d => d.SequenceExists(sequenceName));
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Databases.Any(d => d.SynonymExists(synonymName));
        }

        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            return Databases.Any(d => d.TriggerExists(triggerName));
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableExists = Databases.Select(d => d.TableExistsAsync(tableName)).ToArray();
            var tablePresence = await Task.WhenAll(tableExists);
            return tablePresence.Length > 0;
        }

        public Task<IRelationalDatabaseTable> TableAsync(Identifier tableName) => _tableCache.GetValue(tableName);

        public async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tables = Databases.Select(d => d.TableAsync(tableName)).ToArray();
            var tablesTask = await Task.WhenAll(tables);
            return tablesTask.FirstOrDefault(t => t != null);
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Databases.Select(d => d.TablesAsync()).Concat().Distinct(t => t.Name);
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewExists = Databases.Select(d => d.ViewExistsAsync(viewName)).ToArray();
            var viewPresence = await Task.WhenAll(viewExists);
            return viewPresence.Length > 0;
        }

        public Task<IRelationalDatabaseView> ViewAsync(Identifier viewName) => _viewCache.GetValue(viewName);

        public async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var views = Databases.Select(d => d.ViewAsync(viewName)).ToArray();
            var viewsTask = await Task.WhenAll(views);
            return viewsTask.FirstOrDefault(t => t != null);
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync()
        {
            return Databases.Select(d => d.ViewsAsync()).Concat().Distinct(t => t.Name);
        }

        public async Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequenceExists = Databases.Select(d => d.SequenceExistsAsync(sequenceName)).ToArray();
            var sequencePresence = await Task.WhenAll(sequenceExists);
            return sequencePresence.Length > 0;
        }

        public Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName) => _sequenceCache.GetValue(sequenceName);

        public async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequences = Databases.Select(d => d.SequenceAsync(sequenceName)).ToArray();
            var sequencesTask = await Task.WhenAll(sequences);
            return sequencesTask.FirstOrDefault(t => t != null);
        }

        public IObservable<IDatabaseSequence> SequencesAsync()
        {
            return Databases
                .Select(d => d.SequencesAsync())
                .Concat()
                .Distinct(t => t.Name);
        }

        public async Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymExists = Databases.Select(d => d.SynonymExistsAsync(synonymName)).ToArray();
            var synonymPresence = await Task.WhenAll(synonymExists);
            return synonymPresence.Length > 0;
        }

        public Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName) => _synonymCache.GetValue(synonymName);

        public async Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonyms = Databases.Select(d => d.SynonymAsync(synonymName)).ToArray();
            var synonymsTask = await Task.WhenAll(synonyms);
            return synonymsTask.FirstOrDefault(t => t != null);
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync()
        {
            return Databases.Select(d => d.SynonymsAsync()).Concat().Distinct(t => t.Name);
        }

        public async Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            var synonymExists = Databases.Select(d => d.TriggerExistsAsync(triggerName)).ToArray();
            var synonymPresence = await Task.WhenAll(synonymExists);
            return synonymPresence.Any();
        }

        public Task<IDatabaseTrigger> TriggerAsync(Identifier triggerName) => _triggerCache.GetValue(triggerName);

        public async Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));

            var triggers = Databases.Select(d => d.TriggerAsync(triggerName)).ToArray();
            var triggersTask = await Task.WhenAll(triggers);
            return triggersTask.FirstOrDefault(t => t != null);
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Databases.Select(d => d.TriggersAsync()).Concat().Distinct(t => t.Name);
        }

        private static IDependentRelationalDatabase SetParent(IRelationalDatabase parent, IDependentRelationalDatabase child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = parent;
            return child;
        }

        private readonly AsyncCache<Identifier, IRelationalDatabaseTable> _tableCache;
        private readonly AsyncCache<Identifier, IRelationalDatabaseView> _viewCache;
        private readonly AsyncCache<Identifier, IDatabaseSequence> _sequenceCache;
        private readonly AsyncCache<Identifier, IDatabaseSynonym> _synonymCache;
        private readonly AsyncCache<Identifier, IDatabaseTrigger> _triggerCache;
    }
}
