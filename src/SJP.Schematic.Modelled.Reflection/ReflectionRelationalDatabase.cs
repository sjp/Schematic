using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalDatabase : IRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, Type databaseDefinitionType, string serverName = null, string databaseName = null, string defaultSchema = null)
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

        protected Type DatabaseDefinitionType { get; }

        protected ReflectionTypeProvider TypeProvider { get; }

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Table.TryGetValue(tableName, out var table)
                ? Option<IRelationalDatabaseTable>.Some(table)
                : Option<IRelationalDatabaseTable>.None;
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Table.TryGetValue(tableName, out var table)
                ? OptionAsync<IRelationalDatabaseTable>.Some(table)
                : OptionAsync<IRelationalDatabaseTable>.None;
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables => new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(Table.Count, Table.Values);

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Tables);

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            // TODO: check whether this even exists...
            var table = new ReflectionTable(this, tableType);
            return Option<IRelationalDatabaseTable>.Some(table);
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTableAsync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            // TODO: check whether this even exists...
            var table = new ReflectionTable(this, tableType);
            return OptionAsync<IRelationalDatabaseTable>.Some(table);
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

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return View.TryGetValue(viewName, out var view)
                ? Option<IRelationalDatabaseView>.Some(view)
                : Option<IRelationalDatabaseView>.None;
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return View.TryGetValue(viewName, out var view)
                ? OptionAsync<IRelationalDatabaseView>.Some(view)
                : OptionAsync<IRelationalDatabaseView>.None;
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views => new ReadOnlyCollectionSlim<IRelationalDatabaseView>(View.Count, View.Values);

        public Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Views);

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View => _viewLookup.Value;

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            // TODO: check whether this even exists...
            var view = new ReflectionView(this, viewType);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            // TODO: check whether this even exists...
            var view = new ReflectionView(this, viewType);
            return OptionAsync<IRelationalDatabaseView>.Some(view);
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

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Sequence.TryGetValue(sequenceName, out var sequence)
                ? Option<IDatabaseSequence>.Some(sequence)
                : Option<IDatabaseSequence>.None;
        }

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Sequence.TryGetValue(sequenceName, out var sequence)
                ? OptionAsync<IDatabaseSequence>.Some(sequence)
                : OptionAsync<IDatabaseSequence>.None;
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences => new ReadOnlyCollectionSlim<IDatabaseSequence>(Sequence.Count, Sequence.Values);

        public Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Sequences);

        protected IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            // TODO: check whether this even exists...
            var sequence = new ReflectionSequence(this, sequenceType);
            return Option<IDatabaseSequence>.Some(sequence);
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequenceAsync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            // TODO: check whether this even exists...
            var sequence = new ReflectionSequence(this, sequenceType);
            return OptionAsync<IDatabaseSequence>.Some(sequence);
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

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Synonym.TryGetValue(synonymName, out var synonym)
                ? Option<IDatabaseSynonym>.Some(synonym)
                : Option<IDatabaseSynonym>.None;
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Synonym.TryGetValue(synonymName, out var synonym)
                ? OptionAsync<IDatabaseSynonym>.Some(synonym)
                : OptionAsync<IDatabaseSynonym>.None;
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms => new ReadOnlyCollectionSlim<IDatabaseSynonym>(Synonym.Count, Synonym.Values);

        public Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Synonyms);

        protected IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            // TODO: check whether this even exists...
            var synonym = new ReflectionSynonym(this, synonymType);
            return Option<IDatabaseSynonym>.Some(synonym);
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonymAsync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            // TODO: check whether this even exists...
            var synonym = new ReflectionSynonym(this, synonymType);
            return OptionAsync<IDatabaseSynonym>.Some(synonym);
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

        protected (IEnumerable<string> quotedTypeNames, IReadOnlyDictionary<Identifier, TValue> lookup) CreateLookup<TValue>(IReadOnlyCollection<Option<TValue>> objects) where TValue : IDatabaseEntity
        {
            var result = new Dictionary<Identifier, TValue>(objects.Count);

            var duplicateNames = new System.Collections.Generic.HashSet<Identifier>();
            foreach (var obj in objects)
            {
                obj.IfSome(o =>
                {
                    if (result.ContainsKey(o.Name))
                        duplicateNames.Add(o.Name);

                    result[o.Name] = o;
                });
            }

            var duplicatedTypeNames = duplicateNames.Select(n => Dialect.QuoteName(n.ToString()));
            var lookup = result;

            return (duplicatedTypeNames, lookup);
        }

        // makes no sense to have duplicate types
        private static void EnsureUniqueTypes(Type definitionType, ReflectionTypeProvider typeProvider)
        {
            var foundTypes = new System.Collections.Generic.HashSet<Type>();
            var duplicateTypes = new System.Collections.Generic.HashSet<Type>();

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
