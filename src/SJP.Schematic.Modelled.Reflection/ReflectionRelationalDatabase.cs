using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalDatabase : IRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, Type databaseDefinitionType, string serverName = null, string databaseName = null, string defaultSchema = null, IEqualityComparer<Identifier> comparer = null)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            DatabaseDefinitionType = databaseDefinitionType ?? throw new ArgumentNullException(nameof(databaseDefinitionType));

            if (serverName.IsNullOrWhiteSpace())
                serverName = null;
            ServerName = serverName;

            if (databaseName.IsNullOrWhiteSpace())
                databaseName = null;
            DatabaseName = databaseName;

            if (defaultSchema.IsNullOrWhiteSpace())
                defaultSchema = null;
            DefaultSchema = defaultSchema;

            Comparer = comparer ?? new IdentifierComparer(StringComparer.OrdinalIgnoreCase, serverName, databaseName, defaultSchema);

            TypeProvider = new ReflectionTypeProvider(dialect, databaseDefinitionType);
            EnsureUniqueTypes(DatabaseDefinitionType, TypeProvider);

            _tableLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>>(LoadTables);
            _viewLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>>(LoadViews);
            _sequenceLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>>(LoadSequences);
            _synonymLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>>(LoadSynonyms);
        }

        public IDatabaseDialect Dialect { get; }

        public string ServerName { get; }

        public string DatabaseName { get; }

        public string DefaultSchema { get; }

        public string DatabaseVersion { get; } = string.Empty;

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected Type DatabaseDefinitionType { get; }

        protected ReflectionTypeProvider TypeProvider { get; }

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Table.ContainsKey(tableName);
        }

        public Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var lookupContains = Table.ContainsKey(tableName);
            return Task.FromResult(lookupContains);
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Table.TryGetValue(tableName, out var table) ? table : null;
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var lookupResult = Table.TryGetValue(tableName, out var table) ? table : null;
            return Task.FromResult(lookupResult);
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables => new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(Table.Count, Table.Values);

        public Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IRelationalDatabaseTable>>>(
            Tables.Select(Task.FromResult).ToList()
        );

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        protected virtual IRelationalDatabaseTable LoadTableSync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            return new ReflectionTable(this, tableType);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            var table = new ReflectionTable(this, tableType);
            return Task.FromResult<IRelationalDatabaseTable>(table);
        }

        protected virtual IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> LoadTables()
        {
            var tables = TypeProvider.Tables.Select(LoadTableSync).ToList();
            if (tables.Count == 0)
                return _emptyTableLookup;

            var (duplicateNames, lookup) = CreateLookup(tables);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following tables: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return View.ContainsKey(viewName);
        }

        public Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var lookupContains = View.ContainsKey(viewName);
            return Task.FromResult(lookupContains);
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return View.TryGetValue(viewName, out var view) ? view : null;
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var lookupResult = View.TryGetValue(viewName, out var view) ? view : null;
            return Task.FromResult(lookupResult);
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views => new ReadOnlyCollectionSlim<IRelationalDatabaseView>(View.Count, View.Values);

        public Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IRelationalDatabaseView>>>(
            Views.Select(Task.FromResult).ToList()
        );

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup.Value;

        protected virtual IRelationalDatabaseView LoadViewSync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            return new ReflectionView(this, viewType);
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            var view = new ReflectionView(this, viewType);
            return Task.FromResult<IRelationalDatabaseView>(view);
        }

        protected virtual IReadOnlyDictionary<Identifier, IRelationalDatabaseView> LoadViews()
        {
            var views = TypeProvider.Views.Select(LoadViewSync).ToList();
            if (views.Count == 0)
                return _emptyViewLookup;

            var (duplicateNames, lookup) = CreateLookup(views);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following views: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Sequence.ContainsKey(sequenceName);
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var lookupContains = Sequence.ContainsKey(sequenceName);
            return Task.FromResult(lookupContains);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Sequence.TryGetValue(sequenceName, out var sequence) ? sequence : null;
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var lookupResult = Sequence.TryGetValue(sequenceName, out var sequence) ? sequence : null;
            return Task.FromResult(lookupResult);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences => new ReadOnlyCollectionSlim<IDatabaseSequence>(Sequence.Count, Sequence.Values);

        public Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IDatabaseSequence>>>(
            Sequences.Select(Task.FromResult).ToList()
        );

        protected IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        protected virtual IDatabaseSequence LoadSequenceSync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            return new ReflectionSequence(this, sequenceType);
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            var sequence = new ReflectionSequence(this, sequenceType);
            return Task.FromResult<IDatabaseSequence>(sequence);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSequence> LoadSequences()
        {
            var sequences = TypeProvider.Sequences.Select(LoadSequenceSync).ToList();
            if (sequences.Count == 0)
                return _emptySequenceLookup;

            var (duplicateNames, lookup) = CreateLookup(sequences);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following sequences: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Synonym.ContainsKey(synonymName);
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var lookupContains = Synonym.ContainsKey(synonymName);
            return Task.FromResult(lookupContains);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Synonym.TryGetValue(synonymName, out var synonym) ? synonym : null;
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var lookupResult = Synonym.TryGetValue(synonymName, out var synonym) ? synonym : null;
            return Task.FromResult(lookupResult);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms => new ReadOnlyCollectionSlim<IDatabaseSynonym>(Synonym.Count, Synonym.Values);

        public Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(
            Synonyms.Select(Task.FromResult).ToList()
        );

        protected IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        protected virtual IDatabaseSynonym LoadSynonymSync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            return new ReflectionSynonym(this, synonymType);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            var synonym = new ReflectionSynonym(this, synonymType);
            return Task.FromResult<IDatabaseSynonym>(synonym);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSynonym> LoadSynonyms()
        {
            var synonyms = TypeProvider.Synonyms.Select(LoadSynonymSync).ToList();
            if (synonyms.Count == 0)
                return _emptySynonymLookup;

            var (duplicateNames, lookup) = CreateLookup(synonyms);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following synonyms: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            return Identifier.CreateQualifiedIdentifier(serverName, databaseName, schema, identifier.LocalName);
        }

        protected (IEnumerable<string> quotedTypeNames, IReadOnlyDictionary<Identifier, TValue> lookup) CreateLookup<TValue>(IReadOnlyCollection<TValue> objects) where TValue : IDatabaseEntity
        {
            var result = new Dictionary<Identifier, TValue>(objects.Count, Comparer);

            var duplicateNames = new HashSet<Identifier>();
            foreach (var obj in objects)
            {
                if (result.ContainsKey(obj.Name))
                    duplicateNames.Add(obj.Name);

                result[obj.Name] = obj;
            }

            var duplicatedTypeNames = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
            var lookup = result;

            return (duplicatedTypeNames, lookup);
        }

        // makes no sense to have duplicate types
        private static void EnsureUniqueTypes(Type definitionType, ReflectionTypeProvider typeProvider)
        {
            var foundTypes = new HashSet<Type>();
            var duplicateTypes = new HashSet<Type>();

            var unwrappedTypes = typeProvider.Tables
                .Concat(typeProvider.Views)
                .Concat(typeProvider.Sequences)
                .Concat(typeProvider.Synonyms);

            foreach (var unwrappedType in unwrappedTypes)
            {
                if (!foundTypes.Add(unwrappedType))
                    duplicateTypes.Add(unwrappedType);
            }

            if (duplicateTypes.Count > 0)
            {
                var typeNames = duplicateTypes.Select(t => t.FullName).Join(", ");
                var message = $"There were duplicate types provided to the { definitionType.FullName } database. The following types have more than one declaration: " + typeNames;
                throw new Exception(message);
            }
        }

        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>> _tableLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>> _viewLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>> _sequenceLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>> _synonymLookup;

        private readonly static IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> _emptyTableLookup = new Dictionary<Identifier, IRelationalDatabaseTable>();
        private readonly static IReadOnlyDictionary<Identifier, IRelationalDatabaseView> _emptyViewLookup = new Dictionary<Identifier, IRelationalDatabaseView>();
        private readonly static IReadOnlyDictionary<Identifier, IDatabaseSequence> _emptySequenceLookup = new Dictionary<Identifier, IDatabaseSequence>();
        private readonly static IReadOnlyDictionary<Identifier, IDatabaseSynonym> _emptySynonymLookup = new Dictionary<Identifier, IDatabaseSynonym>();
    }
}
