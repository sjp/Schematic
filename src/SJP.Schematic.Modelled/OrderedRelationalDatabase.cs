using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.Modelled
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

            Databases = databases.Select(d => SetParent(this, d)).ToList();
            BaseDatabase = Databases.Last();
        }

        public IDatabaseDialect Dialect => BaseDatabase.Dialect;

        protected IRelationalDatabase BaseDatabase { get; }

        protected IEnumerable<IRelationalDatabase> Databases { get; }

        public string DefaultSchema => BaseDatabase.DefaultSchema;

        public string ServerName => BaseDatabase.ServerName;

        public string DatabaseName => BaseDatabase.DatabaseName;

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Databases.Any(d => d.TableExists(tableName));
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableExists = Databases.Select(d => d.TableExistsAsync(tableName)).ToArray();
            var tablePresence = await Task.WhenAll(tableExists).ConfigureAwait(false);
            return tablePresence.Length > 0;
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsync(tableName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables =>
            Databases
                .SelectMany(d => d.Tables)
                .DistinctBy(t => t.Name);

        public Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            var result = Databases
                .SelectMany(d => d.Tables)
                .DistinctBy(t => t.Name)
                .ToAsyncEnumerable();
            return Task.FromResult(result);
        }

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var db = Databases.FirstOrDefault(d => d.TableExists(tableName));
            return db?.GetTable(tableName);
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tables = Databases.Select(d => d.GetTableAsync(tableName)).ToArray();
            var tablesTask = await Task.WhenAll(tables).ConfigureAwait(false);
            return Array.Find(tablesTask, t => t != null);
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Databases.Any(d => d.ViewExists(viewName));
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewExists = Databases.Select(d => d.ViewExistsAsync(viewName)).ToArray();
            var viewPresence = await Task.WhenAll(viewExists).ConfigureAwait(false);
            return viewPresence.Length > 0;
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsync(viewName);
        }

        public IEnumerable<IRelationalDatabaseView> Views =>
            Databases
                .SelectMany(d => d.Views)
                .DistinctBy(t => t.Name);

        public Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            var result = Databases
                .SelectMany(d => d.Views)
                .DistinctBy(v => v.Name)
                .ToAsyncEnumerable();
            return Task.FromResult(result);
        }

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var db = Databases.FirstOrDefault(d => d.ViewExists(viewName));
            return db?.GetView(viewName);
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var views = Databases.Select(d => d.GetViewAsync(viewName)).ToArray();
            var viewsTask = await Task.WhenAll(views).ConfigureAwait(false);
            return Array.Find(viewsTask, t => t != null);
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Databases.Any(d => d.SequenceExists(sequenceName));
        }

        public async Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequenceExists = Databases.Select(d => d.SequenceExistsAsync(sequenceName)).ToArray();
            var sequencePresence = await Task.WhenAll(sequenceExists).ConfigureAwait(false);
            return sequencePresence.Length > 0;
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences =>
            Databases
                .SelectMany(d => d.Sequences)
                .DistinctBy(t => t.Name);

        public Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync()
        {
            var result = Databases
                .SelectMany(d => d.Sequences)
                .DistinctBy(s => s.Name)
                .ToAsyncEnumerable();
            return Task.FromResult(result);
        }

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var db = Databases.FirstOrDefault(d => d.SequenceExists(sequenceName));
            return db?.GetSequence(sequenceName);
        }

        protected virtual async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequences = Databases.Select(d => d.GetSequenceAsync(sequenceName)).ToArray();
            var sequencesTask = await Task.WhenAll(sequences).ConfigureAwait(false);
            return Array.Find(sequencesTask, t => t != null);
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Databases.Any(d => d.SynonymExists(synonymName));
        }

        public async Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymExists = Databases.Select(d => d.SynonymExistsAsync(synonymName)).ToArray();
            var synonymPresence = await Task.WhenAll(synonymExists).ConfigureAwait(false);
            return synonymPresence.Length > 0;
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsync(synonymName);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms =>
            Databases
                .SelectMany(d => d.Synonyms)
                .DistinctBy(t => t.Name);

        public Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync()
        {
            var result = Databases
                .SelectMany(d => d.Synonyms)
                .DistinctBy(s => s.Name)
                .ToAsyncEnumerable();
            return Task.FromResult(result);
        }

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var db = Databases.FirstOrDefault(d => d.SynonymExists(synonymName));
            return db?.GetSynonym(synonymName);
        }

        protected virtual async Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonyms = Databases.Select(d => d.GetSynonymAsync(synonymName)).ToArray();
            var synonymsTask = await Task.WhenAll(synonyms).ConfigureAwait(false);
            return Array.Find(synonymsTask, t => t != null);
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
