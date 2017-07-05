using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.Core
{
    public class CachedRelationalDatabase : IRelationalDatabase
    {
        public CachedRelationalDatabase(IRelationalDatabase database, IdentifierComparer comparer = null)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, database.DefaultSchema);

            Table = new IdentifierLookup<IRelationalDatabaseTable>(LoadTableAsync, comparer);
            View = new IdentifierLookup<IRelationalDatabaseView>(LoadViewAsync, comparer);
            Sequence = new IdentifierLookup<IDatabaseSequence>(LoadSequenceAsync, comparer);
            Synonym = new IdentifierLookup<IDatabaseSynonym>(LoadSynonymAsync, comparer);
            Trigger = new IdentifierLookup<IDatabaseTrigger>(LoadTriggerAsync, comparer);
        }

        public IDatabaseDialect Dialect => Database.Dialect;

        protected IRelationalDatabase Database { get; }

        public string DefaultSchema => Database.DefaultSchema;

        public string DatabaseName => Database.DatabaseName;

        #region Tables

        protected IReadOnlyDictionaryAsync<Identifier, IRelationalDatabaseTable> Table { get; }

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return Table.ContainsKey(tableName);
        }

        public Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return Table.ContainsKeyAsync(tableName);
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            Table.TryGetValue(tableName, out var table);
            return table;
        }

        public async Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            var result = await Table.TryGetValueAsync(tableName);
            return result.Value;
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                return Database.Tables
                    .Select(t => Table[t.Name])
                    .Where(t => t != null);
            }
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Database.TablesAsync()
                .Select(t => Table[t.Name])
                .Where(t => t != null);
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Database.GetTable(tableName);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Database.GetTableAsync(tableName);
        }

        #endregion Tables

        #region Views

        protected IReadOnlyDictionaryAsync<Identifier, IRelationalDatabaseView> View { get; }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return View.ContainsKey(viewName);
        }

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return Task.FromResult(View.ContainsKey(viewName));
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            View.TryGetValue(viewName, out var view);
            return view;
        }

        public async Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            var result = await View.TryGetValueAsync(viewName);
            return result.Value;
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                return Database.Views
                    .Select(v => View[v.Name])
                    .Where(v => v != null);
            }
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync()
        {
            return Database.ViewsAsync()
                .Select(v => View[v.Name])
                .Where(v => v != null);
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return Database.GetView(viewName);
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return Database.GetViewAsync(viewName);
        }

        #endregion Views

        #region Sequences

        protected IReadOnlyDictionaryAsync<Identifier, IDatabaseSequence> Sequence { get; }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.ContainsKey(sequenceName);
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.ContainsKeyAsync(sequenceName);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            Sequence.TryGetValue(sequenceName, out var sequence);
            return sequence;
        }

        public async Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            var result = await Sequence.TryGetValueAsync(sequenceName);
            return result.Value;
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                return Database.Sequences
                    .Select(s => Sequence[s.Name])
                    .Where(s => s != null);
            }
        }

        public IObservable<IDatabaseSequence> SequencesAsync()
        {
            return Database.SequencesAsync()
                .Select(s => Sequence[s.Name])
                .Where(s => s != null);
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Database.GetSequence(sequenceName);
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Database.GetSequenceAsync(sequenceName);
        }

        #endregion Sequences

        #region Synonyms

        protected IReadOnlyDictionaryAsync<Identifier, IDatabaseSynonym> Synonym { get; }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.ContainsKey(synonymName);
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.ContainsKeyAsync(synonymName);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            Synonym.TryGetValue(synonymName, out var synonym);
            return synonym;
        }

        public async Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            var result = await Synonym.TryGetValueAsync(synonymName);
            return result.Value;
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                return Database.Synonyms
                    .Select(s => Synonym[s.Name])
                    .Where(s => s != null);
            }
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync()
        {
            return Database.SynonymsAsync()
                .Select(s => Synonym[s.Name])
                .Where(s => s != null);
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Database.GetSynonym(synonymName);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Database.GetSynonymAsync(synonymName);
        }

        #endregion Synonyms

        #region Triggers

        protected IReadOnlyDictionaryAsync<Identifier, IDatabaseTrigger> Trigger { get; }

        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return Trigger.ContainsKey(triggerName);
        }

        public Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return Trigger.ContainsKeyAsync(triggerName);
        }

        public IDatabaseTrigger GetTrigger(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            Trigger.TryGetValue(triggerName, out var trigger);
            return trigger;
        }

        public async Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            var result = await Trigger.TryGetValueAsync(triggerName);
            return result.Value;
        }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                return Database.Triggers
                    .Select(t => Trigger[t.Name])
                    .Where(t => t != null);
            }
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Database.TriggersAsync()
                .Select(t => Trigger[t.Name])
                .Where(t => t != null);
        }

        protected virtual IDatabaseTrigger LoadTriggerSync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            return Database.GetTrigger(triggerName);
        }

        protected virtual Task<IDatabaseTrigger> LoadTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            triggerName = CreateQualifiedIdentifier(triggerName);
            return Database.GetTriggerAsync(triggerName);
        }

        #endregion Triggers

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Schema.IsNullOrWhiteSpace() && !DefaultSchema.IsNullOrWhiteSpace()
                ? new Identifier(DefaultSchema, identifier.LocalName)
                : identifier;
        }

        protected static IDependentRelationalDatabase SetParent(IRelationalDatabase parent, IDependentRelationalDatabase child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            return child;
        }
    }
}
