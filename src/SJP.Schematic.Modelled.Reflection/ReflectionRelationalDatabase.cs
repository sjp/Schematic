using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    // TODO: uncomment interface when ready
    public class ReflectionRelationalDatabase : IRelationalDatabase //, IDependentRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, Type databaseDefinitionType, string databaseName = null, string defaultSchema = null, IEqualityComparer<Identifier> comparer = null)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            DatabaseDefinitionType = databaseDefinitionType ?? throw new ArgumentNullException(nameof(databaseDefinitionType));

            if (databaseName.IsNullOrWhiteSpace())
                databaseName = null;
            DatabaseName = databaseName;

            if (defaultSchema.IsNullOrWhiteSpace())
                defaultSchema = null;
            DefaultSchema = defaultSchema;

            Comparer = comparer ?? new IdentifierComparer(StringComparer.OrdinalIgnoreCase, defaultSchema);

            EnsureUniqueTypes(DatabaseDefinitionType);

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

        public string DatabaseName { get; }

        public string DefaultSchema { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected Type DatabaseDefinitionType { get; }

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
            return Table.ContainsKey(tableName) ? Table[tableName] : null;
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var lookupResult = Table.ContainsKey(tableName) ? Table[tableName] : null;
            return Task.FromResult(lookupResult);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables => Table.Values;

        public IObservable<IRelationalDatabaseTable> TablesAsync() => Tables.ToObservable();

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
            var result = new Dictionary<Identifier, IRelationalDatabaseTable>(Comparer);

            var tables = GetUnwrappedPropertyTypes(TableGenericType)
                .Select(LoadTableSync)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var table in tables)
            {
                if (result.ContainsKey(table.Name))
                    duplicateNames.Add(table.Name);

                result[table.Name] = table;
            }

            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
                var message = "Duplicates found for the following tables: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.AsReadOnlyDictionary();
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
            return View.ContainsKey(viewName) ? View[viewName] : null;
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var view = View.ContainsKey(viewName) ? View[viewName] : null;
            return Task.FromResult(view);
        }

        public IEnumerable<IRelationalDatabaseView> Views => View.Values;

        public IObservable<IRelationalDatabaseView> ViewsAsync() => Views.ToObservable();

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
            var result = new Dictionary<Identifier, IRelationalDatabaseView>(Comparer);

            var views = GetUnwrappedPropertyTypes(ViewGenericType)
                .Select(LoadViewSync)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var view in views)
            {
                if (result.ContainsKey(view.Name))
                    duplicateNames.Add(view.Name);

                result[view.Name] = view;
            }

            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
                var message = "Duplicates found for the following views: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.AsReadOnlyDictionary();
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
            return Sequence.ContainsKey(sequenceName) ? Sequence[sequenceName] : null;
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var sequence = Sequence.ContainsKey(sequenceName) ? Sequence[sequenceName] : null;
            return Task.FromResult(sequence);
        }

        public IEnumerable<IDatabaseSequence> Sequences => Sequence.Values;

        public IObservable<IDatabaseSequence> SequencesAsync() => Sequences.ToObservable();

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
            var result = new Dictionary<Identifier, IDatabaseSequence>(Comparer);

            var sequences = GetUnwrappedPropertyTypes(SequenceGenericType)
                .Select(LoadSequenceSync)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var sequence in sequences)
            {
                if (result.ContainsKey(sequence.Name))
                    duplicateNames.Add(sequence.Name);

                result[sequence.Name] = sequence;
            }

            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
                var message = "Duplicates found for the following sequences: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.AsReadOnlyDictionary();
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
            return Synonym.ContainsKey(synonymName) ? Synonym[synonymName] : null;
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var synonym = Synonym.ContainsKey(synonymName) ? Synonym[synonymName] : null;
            return Task.FromResult(synonym);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms => Synonym.Values;

        public IObservable<IDatabaseSynonym> SynonymsAsync() => Synonyms.ToObservable();

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
            var result = new Dictionary<Identifier, IDatabaseSynonym>(Comparer);

            var synonyms = GetUnwrappedPropertyTypes(SynonymGenericType)
                .Select(LoadSynonymSync)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var synonym in synonyms)
            {
                if (result.ContainsKey(synonym.Name))
                    duplicateNames.Add(synonym.Name);

                result[synonym.Name] = synonym;
            }

            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
                var message = "Duplicates found for the following synonyms: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.AsReadOnlyDictionary();
        }

        #endregion Synonyms

        // TODO: Implement triggers
        //       Pull from embedded resource preferred but not required
        #region Triggers

        public bool TriggerExists(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            throw new NotImplementedException();
        }

        public Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            throw new NotImplementedException();
        }

        public IDatabaseTrigger GetTrigger(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            throw new NotImplementedException();
        }

        public Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName)
        {
            if (triggerName == null || triggerName.LocalName == null)
                throw new ArgumentNullException(nameof(triggerName));

            throw new NotImplementedException();
        }

        public IEnumerable<IDatabaseTrigger> Triggers => throw new NotImplementedException();

        public IObservable<IDatabaseTrigger> TriggersAsync() => Triggers.ToObservable();

        // TODO IMPLEMENT
        protected IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => null; //_triggerLookup.Value;

        protected virtual IDatabaseTrigger LoadTriggerSync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            return null;// new ReflectionSynonym(Database, synonymType);
        }

        protected virtual Task<IDatabaseTrigger> LoadTriggerAsync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            //var synonym = new ReflectionSynonym(Database, synonymType);
            return Task.FromResult<IDatabaseTrigger>(null);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggers()
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(Comparer);

            // TODO! this needs to be changed away from synonyms
            var triggers = GetUnwrappedPropertyTypes(SynonymGenericType)
                .Select(LoadTriggerSync)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var trigger in triggers)
            {
                if (result.ContainsKey(trigger.Name))
                    duplicateNames.Add(trigger.Name);

                result[trigger.Name] = trigger;
            }

            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
                var message = "Duplicates found for the following triggers: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.AsReadOnlyDictionary();
        }

        #endregion Triggers

        protected IEnumerable<Type> GetUnwrappedPropertyTypes(Type objectType)
        {
            if (objectType == null)
                throw new ArgumentNullException(nameof(objectType));

            return DatabaseDefinitionType.GetTypeInfo().GetProperties()
                .Where(pi => pi.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
                .Select(pi => UnwrapGenericParameter(pi.PropertyType))
                .ToList();
        }

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName == null)
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Schema.IsNullOrWhiteSpace() && !DefaultSchema.IsNullOrWhiteSpace()
                ? new Identifier(DefaultSchema, identifier.LocalName)
                : identifier;
        }

        protected static Type TableGenericType { get; } = typeof(Table<>);

        protected static Type ViewGenericType { get; } = typeof(View<>);

        protected static Type SequenceGenericType { get; } = typeof(Sequence<>);

        protected static Type SynonymGenericType { get; } = typeof(Synonym<>);

        protected static Type UnwrapGenericParameter(Type inputType) => inputType.GetTypeInfo().GetGenericArguments().Single();

        // makes no sense to have duplicate types
        private static void EnsureUniqueTypes(Type definitionType)
        {
            var foundTypes = new HashSet<Type>();
            var duplicateTypes = new HashSet<Type>();

            var validWrappers = new[] { TableGenericType, ViewGenericType, SequenceGenericType, SynonymGenericType };
            var unwrappedTypes = definitionType.GetTypeInfo().GetProperties()
                .Where(pi => validWrappers.Any(wrapperType => pi.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(wrapperType.GetTypeInfo())))
                .Select(pi => UnwrapGenericParameter(pi.PropertyType));

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
