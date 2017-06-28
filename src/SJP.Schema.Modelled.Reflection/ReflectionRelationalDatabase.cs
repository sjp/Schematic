using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class ReflectionRelationalDatabase : IRelationalDatabase //, IDependentRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, Type databaseDefinitionType, string databaseName = null, string defaultSchema = null)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            DatabaseDefinitionType = databaseDefinitionType ?? throw new ArgumentNullException(nameof(databaseDefinitionType));

            if (databaseName.IsNullOrWhiteSpace())
                databaseName = null;
            DatabaseName = databaseName;

            if (defaultSchema.IsNullOrWhiteSpace())
                defaultSchema = null;
            DefaultSchema = defaultSchema;

            EnsureUniqueTypes(DatabaseDefinitionType);

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

        public IDatabaseDialect Dialect { get; }

        public string DatabaseName { get; }

        public string DefaultSchema { get; }

        protected Type DatabaseDefinitionType { get; }

        #region Tables

        public bool TableExists(Identifier tableName)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            return Table.ContainsKey(tableName);
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        private IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> LoadTableLookup()
        {
            var result = new Dictionary<Identifier, IRelationalDatabaseTable>();

            var tables = GetUnwrappedPropertyTypes(TableGenericType)
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

            var views = GetUnwrappedPropertyTypes(ViewGenericType)
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

            var sequences = GetUnwrappedPropertyTypes(SequenceGenericType)
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

            var synonyms = GetUnwrappedPropertyTypes(SynonymGenericType)
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
            tableName = CreateQualifiedIdentifier(tableName);

            var lookupContains = Table.ContainsKey(tableName);
            return Task.FromResult(lookupContains);
        }

        public Task<IRelationalDatabaseTable> TableAsync(Identifier tableName)
        {
            tableName = CreateQualifiedIdentifier(tableName);

            var lookupResult = Table.ContainsKey(tableName) ? Table[tableName] : null;
            return Task.FromResult(lookupResult);
        }

        public IObservable<IRelationalDatabaseTable> TablesAsync() => Tables.ToObservable();

        protected virtual IRelationalDatabaseTable LoadTable(Type tableType)
        {
            return new ReflectionTable(this, tableType);
        }

        protected virtual IRelationalDatabaseView LoadView(Type viewType)
        {
            return new ReflectionView(this, viewType);
        }

        protected virtual IDatabaseSequence LoadSequence(Type sequenceType)
        {
            return new ReflectionSequence(this, sequenceType);
        }

        protected virtual IDatabaseSynonym LoadSynonym(Type synonymType)
        {
            return new ReflectionSynonym(this, synonymType);
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            tableName = CreateQualifiedIdentifier(tableName);

            var table = Table[tableName];
            return Task.FromResult(table);
        }

        #endregion Tables

        #region Views

        public bool ViewExists(Identifier viewName)
        {
            viewName = CreateQualifiedIdentifier(viewName);
            return View.ContainsKey(viewName);
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup.Value;

        public IEnumerable<IRelationalDatabaseView> Views => View.Values;

        public Task<bool> ViewExistsAsync(Identifier viewName)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            var lookupContains = View.ContainsKey(viewName);
            return Task.FromResult(lookupContains);
        }

        public Task<IRelationalDatabaseView> ViewAsync(Identifier viewName)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            var view = View[viewName];
            return Task.FromResult(view);
        }

        public IObservable<IRelationalDatabaseView> ViewsAsync() => Views.ToObservable();

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            var view = View[viewName];
            return Task.FromResult(view);
        }

        #endregion Views

        #region Sequences

        public bool SequenceExists(Identifier sequenceName)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Sequence.ContainsKey(sequenceName);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        public IEnumerable<IDatabaseSequence> Sequences => Sequence.Values;

        public Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var lookupContains = Sequence.ContainsKey(sequenceName);
            return Task.FromResult(lookupContains);
        }

        public Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var sequence = Sequence[sequenceName];
            return Task.FromResult(sequence);
        }

        public IObservable<IDatabaseSequence> SequencesAsync() => Sequences.ToObservable();

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var sequence = Sequence[sequenceName];
            return Task.FromResult(sequence);
        }

        #endregion Sequences

        #region Synonyms

        public bool SynonymExists(Identifier synonymName)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);
            return Synonym.ContainsKey(synonymName);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        public IEnumerable<IDatabaseSynonym> Synonyms => Synonym.Values;

        public Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            var lookupContains = Synonym.ContainsKey(synonymName);
            return Task.FromResult(lookupContains);
        }

        public Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            var synonym = Synonym[synonymName];
            return Task.FromResult(synonym);
        }

        public IObservable<IDatabaseSynonym> SynonymsAsync() => Synonyms.ToObservable();

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            var synonym = Synonym[synonymName];
            return Task.FromResult(synonym);
        }

        #endregion Synonyms

        protected static Type TableGenericType { get; } = typeof(Table<>);

        protected static Type ViewGenericType { get; } = typeof(View<>);

        protected static Type SequenceGenericType { get; } = typeof(Sequence<>);

        protected static Type SynonymGenericType { get; } = typeof(Synonym<>);

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
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Schema.IsNullOrWhiteSpace() && !DefaultSchema.IsNullOrWhiteSpace()
                ? new Identifier(DefaultSchema, identifier.LocalName)
                : identifier;
        }

        private IRelationalDatabase _parentDb;

        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>> _tableLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseView>> _viewLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>> _sequenceLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>> _synonymLookup;
    }
}
