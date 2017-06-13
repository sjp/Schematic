using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SJP.Schema.Core;
using SJP.Schema.Core.Utilities;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    // TODO: uncomment interface when ready
    public class ReflectionRelationalDatabase<T> : IRelationalDatabase, IReflectionRelationalDatabase //, IDependentRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));

            EnsureUniqueTypes(DatabaseDefinitionType);

            var tableInstances = LoadAllGenericTypes(DatabaseDefinitionType, TableGenericType);
            TableInstances = InitializeTables(tableInstances);

            ViewInstances = LoadAllGenericTypes(DatabaseDefinitionType, ViewGenericType);
            SequenceInstances = LoadAllGenericTypes(DatabaseDefinitionType, SequenceGenericType);
            SynonymInstances = LoadAllGenericTypes(DatabaseDefinitionType, SynonymGenericType);

            _parentDb = this; // TODO replace other uses of 'this' with 'Database' because means we can override

            _tableLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>>(LoadTableLookup);
            _viewLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>>(LoadViewLookup);
            _sequenceLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>>(LoadSequenceLookup);
            _synonymLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>>(LoadSynonymLookup);
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IRelationalDatabase Database => Parent;

        private static IReadOnlyDictionary<Type, object> LoadAllGenericTypes(Type databaseType, Type genericDefinition)
        {
            var result = new Dictionary<Type, object>();

            var typeArgs = databaseType.GetTypeInfo().DeclaredProperties
                .Where(pi => pi.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(genericDefinition.GetTypeInfo()))
                .Select(pi => UnwrapGenericParameter(pi.PropertyType));

            foreach (var typeArg in typeArgs)
            {
                var instance = Activator.CreateInstance(typeArg);
                result[typeArg] = instance;
            }

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<Type, object> InitializeTables(IEnumerable<KeyValuePair<Type, object>> tables)
        {
            var genericType = typeof(ReflectionTable<>);
            foreach (var table in tables)
            {
                var specificType = genericType.MakeGenericType(table.Key);
                var populateMethod = specificType.GetTypeInfo().GetMethod(nameof(ReflectionTable<object>.PopulateProperties), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                populateMethod.Invoke(null, new[] { table.Value });
            }

            return tables.ToImmutableDictionary();
        }

        public IDatabaseDialect Dialect { get; }

        // TODO: maybe pull metadata from dialect?
        //       dialect can have a connection
        public string DatabaseName => string.Empty;

        public string DefaultSchema => "dbo"; // TODO this is definitely wrong

        protected Type DatabaseDefinitionType { get; } = typeof(T);

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            return Table.ContainsKey(tableName);
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        private IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> LoadTableLookup()
        {
            var result = new Dictionary<Identifier, IRelationalDatabaseTable>();

            var tables = TableInstances
                .Keys
                .Select(LoadTable)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var table in tables)
            {
                if (result.ContainsKey(table.Name))
                    duplicateNames.Add(table.Name);

                result[table.Name] = table;
            }

            // TODO! quote the names
            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => n.ToString());
                var message = "Duplicates found for the following tables: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return new IdentifierLookup<IRelationalDatabaseTable>(DefaultSchema, result.ToImmutableDictionary());
        }

        private IReadOnlyDictionary<Identifier, IRelationalDatabaseView> LoadViewLookup()
        {
            var result = new Dictionary<Identifier, IRelationalDatabaseView>();

            var views = ViewInstances
                .Keys
                .Select(LoadView)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var view in views)
            {
                if (result.ContainsKey(view.Name))
                    duplicateNames.Add(view.Name);

                result[view.Name] = view;
            }

            // TODO! quote the names
            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => n.ToString());
                var message = "Duplicates found for the following views: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseSequence> LoadSequenceLookup()
        {
            var result = new Dictionary<Identifier, IDatabaseSequence>();

            var sequences = SequenceInstances.Keys
                .Select(LoadSequence)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var sequence in sequences)
            {
                if (result.ContainsKey(sequence.Name))
                    duplicateNames.Add(sequence.Name);

                result[sequence.Name] = sequence;
            }

            // TODO! quote the names
            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => n.ToString());
                var message = "Duplicates found for the following sequences: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseSynonym> LoadSynonymLookup()
        {
            var result = new Dictionary<Identifier, IDatabaseSynonym>();

            var synonyms = SynonymInstances.Keys
                .Select(LoadSynonym)
                .ToList();

            var duplicateNames = new HashSet<Identifier>();
            foreach (var synonym in synonyms)
            {
                if (result.ContainsKey(synonym.Name))
                    duplicateNames.Add(synonym.Name);

                result[synonym.Name] = synonym;
            }

            // TODO! quote the names
            if (duplicateNames.Count > 0)
            {
                var dupes = duplicateNames.Select(n => n.ToString());
                var message = "Duplicates found for the following synonyms: " + dupes.Join(", ");
                throw new Exception(message);
            }

            return result.ToImmutableDictionary();
        }

        public IEnumerable<IRelationalDatabaseTable> Tables => Table.Values;

        public Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            var lookupContains = Table.ContainsKey(tableName);
            return Task.FromResult(lookupContains);
        }

        public Task<IRelationalDatabaseTable> TableAsync(Identifier tableName)
        {
            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            var lookupResult = Table.ContainsKey(tableName) ? Table[tableName] : null;
            return Task.FromResult(lookupResult);
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync() => Tables.ToObservable();

        protected virtual IRelationalDatabaseTable LoadTable(Type tableType)
        {
            var reflectionTableType = typeof(ReflectionTable<>);
            var specificType = reflectionTableType.MakeGenericType(tableType);

            return (IRelationalDatabaseTable)Activator.CreateInstance(specificType, this);
        }

        protected virtual IRelationalDatabaseView LoadView(Type viewType)
        {
            var reflectionViewType = typeof(ReflectionView<>);
            var specificType = reflectionViewType.MakeGenericType(viewType);

            return (IRelationalDatabaseView)Activator.CreateInstance(specificType, this);
        }

        protected virtual IDatabaseSequence LoadSequence(Type sequenceType)
        {
            var reflectionSequenceType = typeof(ReflectionSequence<>);
            var specificType = reflectionSequenceType.MakeGenericType(sequenceType);

            return (IDatabaseSequence)Activator.CreateInstance(specificType, this);
        }

        protected virtual IDatabaseSynonym LoadSynonym(Type synonymType)
        {
            var reflectionSynonymType = typeof(ReflectionSynonym<>);
            var specificType = reflectionSynonymType.MakeGenericType(synonymType);

            return (IDatabaseSynonym)Activator.CreateInstance(specificType, this);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName.Schema.IsNullOrWhiteSpace())
                tableName = new Identifier(DefaultSchema, tableName.LocalName);

            var table = Table[tableName];
            return Task.FromResult(table);
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            return View.ContainsKey(viewName);
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup.Value;

        public IEnumerable<IRelationalDatabaseView> Views => View.Values;

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            var lookupContains = View.ContainsKey(viewName);
            return Task.FromResult(lookupContains);
        }

        public Task<IRelationalDatabaseView> ViewAsync(Identifier viewName)
        {
            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            var view = View[viewName];
            return Task.FromResult(view);
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync() => Views.ToObservable();

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName.Schema.IsNullOrWhiteSpace())
                viewName = new Identifier(DefaultSchema, viewName.LocalName);

            var view = View[viewName];
            return Task.FromResult(view);
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            return Sequence.ContainsKey(sequenceName);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        public IEnumerable<IDatabaseSequence> Sequences => Sequence.Values;

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            var lookupContains = Sequence.ContainsKey(sequenceName);
            return Task.FromResult(lookupContains);
        }

        public Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName)
        {
            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            var sequence = Sequence[sequenceName];
            return Task.FromResult(sequence);
        }

        public IObservable<IDatabaseSequence> SequencesAsync() => Sequences.ToObservable();

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName.Schema.IsNullOrWhiteSpace())
                sequenceName = new Identifier(DefaultSchema, sequenceName.LocalName);

            var sequence = Sequence[sequenceName];
            return Task.FromResult(sequence);
        }

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            return Synonym.ContainsKey(synonymName);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        public IEnumerable<IDatabaseSynonym> Synonyms => Synonym.Values;

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            var lookupContains = Synonym.ContainsKey(synonymName);
            return Task.FromResult(lookupContains);
        }

        public Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName)
        {
            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            var synonym = Synonym[synonymName];
            return Task.FromResult(synonym);
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync() => Synonyms.ToObservable();

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName.Schema.IsNullOrWhiteSpace())
                synonymName = new Identifier(DefaultSchema, synonymName.LocalName);

            var synonym = Synonym[synonymName];
            return Task.FromResult(synonym);
        }

        #endregion Synonyms

        private static Type TableGenericType { get; } = typeof(Table<>);
        private static Type ViewGenericType { get; } = typeof(View<>);
        private static Type SequenceGenericType { get; } = typeof(Sequence<>);
        private static Type SynonymGenericType { get; } = typeof(Synonym<>);

        public IReadOnlyDictionary<Type, object> TableInstances { get; }
        public IReadOnlyDictionary<Type, object> ViewInstances { get; }
        public IReadOnlyDictionary<Type, object> SequenceInstances { get; }
        public IReadOnlyDictionary<Type, object> SynonymInstances { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IDatabaseTrigger> Triggers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        // makes no sense to have duplicate types
        private static void EnsureUniqueTypes(Type definitionType)
        {
            var foundTypes = new HashSet<Type>();
            var duplicateTypes = new HashSet<Type>();

            var validWrappers = new[] { TableGenericType, ViewGenericType, SequenceGenericType };
            var unwrappedTypes = definitionType.GetTypeInfo().DeclaredProperties
                .Where(pi => validWrappers.Any(wrapperType => pi.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(wrapperType.GetTypeInfo())))
                .Select(pi => UnwrapGenericParameter(pi.PropertyType))
                .ToList();

            foreach (var unwrapped in unwrappedTypes)
            {
                if (!foundTypes.Add(unwrapped))
                    duplicateTypes.Add(unwrapped);
            }

            if (duplicateTypes.Count > 0)
            {
                var typeNames = duplicateTypes.Select(t => t.FullName).Join(", ");
                var message = "There were duplicate types provided to the modelled database. The following types have more than one declaration: " + typeNames;
                throw new Exception(message);
            }
        }

        private static Type UnwrapGenericParameter(Type inputType) => inputType.GetTypeInfo().GetGenericArguments().Single();

        // TODO: Implement triggers
        //       Pull from embedded resource preferred but not required
        #region Triggers

        public bool TriggerExists(Identifier triggerName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TriggerExistsAsync(Identifier triggerName)
        {
            throw new NotImplementedException();
        }

        public Task<IDatabaseTrigger> TriggerAsync(Identifier triggerName)
        {
            throw new NotImplementedException();
        }

        public IObservable<IDatabaseTrigger> TriggersAsync() => Triggers.ToObservable();

        #endregion Triggers

        private IRelationalDatabase _parentDb;

        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>> _tableLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>> _viewLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>> _sequenceLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>> _synonymLookup;
    }
}
