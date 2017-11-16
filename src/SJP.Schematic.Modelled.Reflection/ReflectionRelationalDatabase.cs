using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    // TODO: uncomment interface when ready
    public class ReflectionRelationalDatabase : IRelationalDatabase //, IDependentRelationalDatabase
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

            _parentDb = this;

            _tableLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>>(LoadTables);
            _viewLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>>(LoadViews);
            _sequenceLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>>(LoadSequences);
            _synonymLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>>(LoadSynonyms);
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IRelationalDatabase Database => Parent;

        public IDatabaseDialect Dialect { get; }

        public string ServerName { get; }

        public string DatabaseName { get; }

        public string DefaultSchema { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected Type DatabaseDefinitionType { get; }

        protected ReflectionTypeProvider TypeProvider { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Table.ContainsKey(tableName);
        }

        public Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var lookupContains = Table.ContainsKey(tableName);
            return Task.FromResult(lookupContains);
        }

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return Table.TryGetValue(tableName, out var table) ? table : null;
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var lookupResult = Table.TryGetValue(tableName, out var table) ? table : null;
            return Task.FromResult(lookupResult);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables => Table.Values;

        public Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync() => Task.FromResult(Tables.ToAsyncEnumerable());

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        protected virtual IRelationalDatabaseTable LoadTableSync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            return new ReflectionTable(Database, tableType);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            var table = new ReflectionTable(Database, tableType);
            return Task.FromResult<IRelationalDatabaseTable>(table);
        }

        protected virtual IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> LoadTables()
        {
            var tables = TypeProvider.Tables.Select(LoadTableSync).ToList();
            if (tables.Count == 0)
                return new Dictionary<Identifier, IRelationalDatabaseTable>(Comparer);

            var (duplicateNames, lookup) = CreateLookup(tables);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following tables: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return View.ContainsKey(viewName);
        }

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var lookupContains = View.ContainsKey(viewName);
            return Task.FromResult(lookupContains);
        }

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return View.TryGetValue(viewName, out var view) ? view : null;
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var lookupResult = View.TryGetValue(viewName, out var view) ? view : null;
            return Task.FromResult(lookupResult);
        }

        public IEnumerable<IRelationalDatabaseView> Views => View.Values;

        public Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync() => Task.FromResult(Views.ToAsyncEnumerable());

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup.Value;

        protected virtual IRelationalDatabaseView LoadViewSync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            return new ReflectionView(Database, viewType);
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            var view = new ReflectionView(Database, viewType);
            return Task.FromResult<IRelationalDatabaseView>(view);
        }

        protected virtual IReadOnlyDictionary<Identifier, IRelationalDatabaseView> LoadViews()
        {
            var views = TypeProvider.Views.Select(LoadViewSync).ToList();
            if (views.Count == 0)
                return new Dictionary<Identifier, IRelationalDatabaseView>(Comparer);

            var (duplicateNames, lookup) = CreateLookup(views);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following views: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Sequence.ContainsKey(sequenceName);
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var lookupContains = Sequence.ContainsKey(sequenceName);
            return Task.FromResult(lookupContains);
        }

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return Sequence.TryGetValue(sequenceName, out var sequence) ? sequence : null;
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var lookupResult = Sequence.TryGetValue(sequenceName, out var sequence) ? sequence : null;
            return Task.FromResult(lookupResult);
        }

        public IEnumerable<IDatabaseSequence> Sequences => Sequence.Values;

        public Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync() => Task.FromResult(Sequences.ToAsyncEnumerable());

        protected IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        protected virtual IDatabaseSequence LoadSequenceSync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            return new ReflectionSequence(Database, sequenceType);
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            var sequence = new ReflectionSequence(Database, sequenceType);
            return Task.FromResult<IDatabaseSequence>(sequence);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSequence> LoadSequences()
        {
            var sequences = TypeProvider.Sequences.Select(LoadSequenceSync).ToList();
            if (sequences.Count == 0)
                return new Dictionary<Identifier, IDatabaseSequence>(Comparer);

            var (duplicateNames, lookup) = CreateLookup(sequences);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following sequences: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Synonym.ContainsKey(synonymName);
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var lookupContains = Synonym.ContainsKey(synonymName);
            return Task.FromResult(lookupContains);
        }

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return Synonym.TryGetValue(synonymName, out var synonym) ? synonym : null;
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var lookupResult = Synonym.TryGetValue(synonymName, out var synonym) ? synonym : null;
            return Task.FromResult(lookupResult);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms => Synonym.Values;

        public Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync() => Task.FromResult(Synonyms.ToAsyncEnumerable());

        protected IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        protected virtual IDatabaseSynonym LoadSynonymSync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            return new ReflectionSynonym(Database, synonymType);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            var synonym = new ReflectionSynonym(Database, synonymType);
            return Task.FromResult<IDatabaseSynonym>(synonym);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSynonym> LoadSynonyms()
        {
            var synonyms = TypeProvider.Synonyms.Select(LoadSynonymSync).ToList();
            if (synonyms.Count == 0)
                return new Dictionary<Identifier, IDatabaseSynonym>(Comparer);

            var (duplicateNames, lookup) = CreateLookup(synonyms);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following synonyms: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
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

        protected (IEnumerable<string> quotedTypeNames, IReadOnlyDictionary<Identifier, TValue> lookup) CreateLookup<TValue>(IEnumerable<TValue> objects) where TValue : IDatabaseEntity
        {
            var result = new Dictionary<Identifier, TValue>(Comparer);

            var duplicateNames = new HashSet<Identifier>();
            foreach (var obj in objects)
            {
                if (result.ContainsKey(obj.Name))
                    duplicateNames.Add(obj.Name);

                result[obj.Name] = obj;
            }

            var duplicatedTypeNames = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
            var lookup = result.AsReadOnlyDictionary();

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

        private IRelationalDatabase _parentDb;

        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>> _tableLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>> _viewLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>> _sequenceLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>> _synonymLookup;
    }
}
