using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.Core
{
    public class CachedRelationalDatabase : IRelationalDatabase
    {
        public CachedRelationalDatabase(IRelationalDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));

            Table = new AsyncCache<Identifier, IRelationalDatabaseTable>(LoadTableAsync);
            View = new AsyncCache<Identifier, IRelationalDatabaseView>(LoadViewAsync);
            Sequence = new AsyncCache<Identifier, IDatabaseSequence>(LoadSequenceAsync);
            Synonym = new AsyncCache<Identifier, IDatabaseSynonym>(LoadSynonymAsync);
            Trigger = new AsyncCache<Identifier, IDatabaseTrigger>(LoadTriggerAsync);
        }

        public IDatabaseDialect Dialect => Database.Dialect;

        protected IRelationalDatabase Database { get; }

        public string DefaultSchema => Database.DefaultSchema;

        public string DatabaseName => Database.DatabaseName;

        #region Tables

        protected AsyncCache<Identifier, IRelationalDatabaseTable> Table { get; }

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return Database.TableExists(tableName);
        }

        public Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return Database.TableExistsAsync(tableName);
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsync(tableName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                return Database.Tables
                    .Select(t => Table[t.Name].Result)
                    .Where(t => t != null);
            }
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync()
        {
            return Database.TablesAsync()
                .SelectMany(t => Table[t.Name].ToObservable())
                .Where(t => t != null);
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            return Table[tableName].Result;
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

        protected AsyncCache<Identifier, IRelationalDatabaseView> View { get; }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return Database.ViewExists(viewName);
        }

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return Database.ViewExistsAsync(viewName);
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                return Database.Views
                    .Select(v => View[v.Name].Result)
                    .Where(v => v != null);
            }
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync()
        {
            return Database.ViewsAsync()
                .SelectMany(v => View[v.Name].ToObservable())
                .Where(v => v != null);
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName).Result;
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

        protected AsyncCache<Identifier, IDatabaseSequence> Sequence { get; }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Database.SequenceExists(sequenceName);
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            return Database.SequenceExistsAsync(sequenceName);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                return Database.Sequences
                    .Select(s => Sequence[s.Name].Result)
                    .Where(s => s != null);
            }
        }

        public IObservable<IDatabaseSequence> SequencesAsync()
        {
            return Database.SequencesAsync()
                .SelectMany(s => Sequence[s.Name].ToObservable())
                .Where(s => s != null);
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsync(sequenceName).Result;
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

        protected AsyncCache<Identifier, IDatabaseSynonym> Synonym { get; }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Database.SynonymExists(synonymName);
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Database.SynonymExistsAsync(synonymName);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsync(synonymName);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                return Database.Synonyms
                    .Select(s => Synonym[s.Name].Result)
                    .Where(s => s != null);
            }
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync()
        {
            return Database.SynonymsAsync()
                .SelectMany(s => Synonym[s.Name].ToObservable())
                .Where(s => s != null);
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsync(synonymName).Result;
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

        protected AsyncCache<Identifier, IDatabaseTrigger> Trigger { get; }

        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return Database.TriggerExists(triggerName);
        }

        public Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return Database.TriggerExistsAsync(triggerName);
        }

        public IDatabaseTrigger GetTrigger(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return LoadTriggerSync(triggerName);
        }

        public Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return LoadTriggerAsync(triggerName);
        }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                return Database.Triggers
                    .Select(t => Trigger[t.Name].Result)
                    .Where(t => t != null);
            }
        }

        public IObservable<IDatabaseTrigger> TriggersAsync()
        {
            return Database.TriggersAsync()
                .SelectMany(t => Trigger[t.Name].ToObservable())
                .Where(t => t != null);
        }

        protected virtual IDatabaseTrigger LoadTriggerSync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(triggerName));

            return LoadTriggerAsync(triggerName).Result;
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
