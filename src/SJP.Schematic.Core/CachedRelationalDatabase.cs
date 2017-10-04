using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    public class CachedRelationalDatabase : IRelationalDatabase
    {
        public CachedRelationalDatabase(IRelationalDatabase database, IEqualityComparer<Identifier> comparer = null)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, database.ServerName, database.DatabaseName, database.DefaultSchema);

            Table = new IdentifierKeyedCache<IRelationalDatabaseTable>(LoadTableAsync, Comparer);
            View = new IdentifierKeyedCache<IRelationalDatabaseView>(LoadViewAsync, Comparer);
            Sequence = new IdentifierKeyedCache<IDatabaseSequence>(LoadSequenceAsync, Comparer);
            Synonym = new IdentifierKeyedCache<IDatabaseSynonym>(LoadSynonymAsync, Comparer);
        }

        public IDatabaseDialect Dialect => Database.Dialect;

        protected IRelationalDatabase Database { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public string DefaultSchema => Database.DefaultSchema;

        public string ServerName => Database.ServerName;

        public string DatabaseName => Database.DatabaseName;

        #region Tables

        protected ICache<Identifier, IRelationalDatabaseTable> Table { get; }

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Table.ContainsKey(tableName);
        }

        public Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Table.ContainsKeyAsync(tableName);
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Table.GetValue(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Table.GetValueAsync(tableName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                return Database.Tables
                    .Select(t => Table.GetValue(t.Name))
                    .Where(t => t != null);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            return tables
                .Select(t => Table.GetValue(t.Name))
                .Where(t => t != null);
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var table = Database.GetTable(tableName);

            return new CachedRelationalDatabaseTable(Database, table);
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);

            return new CachedRelationalDatabaseTable(Database, table);
        }

        #endregion Tables

        #region Views

        protected ICache<Identifier, IRelationalDatabaseView> View { get; }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return View.ContainsKey(viewName);
        }

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Task.FromResult(View.ContainsKey(viewName));
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return View.GetValue(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            return View.GetValueAsync(viewName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                return Database.Views
                    .Select(v => View.GetValue(v.Name))
                    .Where(v => v != null);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            var views = await Database.ViewsAsync().ConfigureAwait(false);
            return views
                .Select(v => View.GetValue(v.Name))
                .Where(v => v != null);
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return Database.GetView(viewName);
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return Database.GetViewAsync(viewName);
        }

        #endregion Views

        #region Sequences

        protected ICache<Identifier, IDatabaseSequence> Sequence { get; }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.ContainsKey(sequenceName);
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.ContainsKeyAsync(sequenceName);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.GetValue(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Sequence.GetValueAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                return Database.Sequences
                    .Select(s => Sequence.GetValue(s.Name))
                    .Where(s => s != null);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync()
        {
            var sequences = await Database.SequencesAsync().ConfigureAwait(false);
            return sequences
                .Select(s => Sequence.GetValue(s.Name))
                .Where(s => s != null);
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Database.GetSequence(sequenceName);
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Database.GetSequenceAsync(sequenceName);
        }

        #endregion Sequences

        #region Synonyms

        protected ICache<Identifier, IDatabaseSynonym> Synonym { get; }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.ContainsKey(synonymName);
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.ContainsKeyAsync(synonymName);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.GetValue(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Synonym.GetValueAsync(synonymName);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                return Database.Synonyms
                    .Select(s => Synonym.GetValue(s.Name))
                    .Where(s => s != null);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync()
        {
            var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);
            return synonyms
                .Select(s => Synonym.GetValue(s.Name))
                .Where(s => s != null);
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Database.GetSynonym(synonymName);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Database.GetSynonymAsync(synonymName);
        }

        #endregion Synonyms

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            if (!serverName.IsNullOrWhiteSpace())
                return new Identifier(serverName, databaseName, schema, identifier.LocalName);
            if (!databaseName.IsNullOrWhiteSpace())
                return new Identifier(databaseName, schema, identifier.LocalName);
            if (!schema.IsNullOrWhiteSpace())
                return new Identifier(schema, identifier.LocalName);

            return identifier;
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
