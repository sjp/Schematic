using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalDatabase : IRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, Type databaseDefinitionType, IIdentifierDefaults identifierDefaults)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            DatabaseDefinitionType = databaseDefinitionType ?? throw new ArgumentNullException(nameof(databaseDefinitionType));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

            TypeProvider = new ReflectionTypeProvider(dialect, databaseDefinitionType);
            EnsureUniqueTypes(DatabaseDefinitionType, TypeProvider);

            _tableLookup = new Lazy<IReadOnlyDictionary<Identifier, IRelationalDatabaseTable>>(LoadTables);
            _viewLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseView>>(LoadViews);
            _sequenceLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>>(LoadSequences);
            _synonymLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>>(LoadSynonyms);
            _routineLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseRoutine>>(LoadRoutines);
        }

        public IDatabaseDialect Dialect { get; }

        public IIdentifierDefaults IdentifierDefaults { get; }

        protected Type DatabaseDefinitionType { get; }

        protected ReflectionTypeProvider TypeProvider { get; }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Table.TryGetValue(tableName, out var table)
                ? OptionAsync<IRelationalDatabaseTable>.Some(table)
                : OptionAsync<IRelationalDatabaseTable>.None;
        }

        public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => Table.Values
            .OrderBy(t => t.Name.Schema)
            .ThenBy(t => t.Name.LocalName)
            .ToAsyncEnumerable();

        protected IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table => _tableLookup.Value;

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            // TODO: check whether this even exists...
            var table = new ReflectionTable(this, Dialect, tableType);
            return Option<IRelationalDatabaseTable>.Some(table);
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            // TODO: check whether this even exists...
            var table = new ReflectionTable(this, Dialect, tableType);
            return OptionAsync<IRelationalDatabaseTable>.Some(table);
        }

        protected virtual IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> LoadTables()
        {
            var tables = TypeProvider.Tables.Select(LoadTableSync).ToList();
            if (tables.Empty())
                return EmptyTableLookup;

            var (duplicateNames, lookup) = CreateLookup(tables);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following tables: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return View.TryGetValue(viewName, out var view)
                ? OptionAsync<IDatabaseView>.Some(view)
                : OptionAsync<IDatabaseView>.None;
        }

        public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default) => View.Values
            .OrderBy(v => v.Name.Schema)
            .ThenBy(v => v.Name.LocalName)
            .ToAsyncEnumerable();

        protected IReadOnlyDictionary<Identifier, IDatabaseView> View => _viewLookup.Value;

        protected virtual Option<IDatabaseView> LoadViewSync(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            // TODO: check whether this even exists...
            var view = new ReflectionView(this, Dialect, viewType);
            return Option<IDatabaseView>.Some(view);
        }

        protected virtual OptionAsync<IDatabaseView> LoadView(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            // TODO: check whether this even exists...
            var view = new ReflectionView(this, Dialect, viewType);
            return OptionAsync<IDatabaseView>.Some(view);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseView> LoadViews()
        {
            var views = TypeProvider.Views.Select(LoadViewSync).ToList();
            if (views.Empty())
                return EmptyViewLookup;

            var (duplicateNames, lookup) = CreateLookup(views);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following views: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Sequence.TryGetValue(sequenceName, out var sequence)
                ? OptionAsync<IDatabaseSequence>.Some(sequence)
                : OptionAsync<IDatabaseSequence>.None;
        }

        public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default) => Sequence.Values
            .OrderBy(s => s.Name.Schema)
            .ThenBy(s => s.Name.LocalName)
            .ToAsyncEnumerable();

        protected IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence => _sequenceLookup.Value;

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            // TODO: check whether this even exists...
            var sequence = new ReflectionSequence(this, Dialect, sequenceType);
            return Option<IDatabaseSequence>.Some(sequence);
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Type sequenceType)
        {
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            // TODO: check whether this even exists...
            var sequence = new ReflectionSequence(this, Dialect, sequenceType);
            return OptionAsync<IDatabaseSequence>.Some(sequence);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSequence> LoadSequences()
        {
            var sequences = TypeProvider.Sequences.Select(LoadSequenceSync).ToList();
            if (sequences.Empty())
                return EmptySequenceLookup;

            var (duplicateNames, lookup) = CreateLookup(sequences);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following sequences: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Synonym.TryGetValue(synonymName, out var synonym)
                ? OptionAsync<IDatabaseSynonym>.Some(synonym)
                : OptionAsync<IDatabaseSynonym>.None;
        }

        public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default) => Synonym.Values
            .OrderBy(s => s.Name.Schema)
            .ThenBy(s => s.Name.LocalName)
            .ToAsyncEnumerable();

        protected IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym => _synonymLookup.Value;

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            // TODO: check whether this even exists...
            var synonym = new ReflectionSynonym(this, Dialect, synonymType);
            return Option<IDatabaseSynonym>.Some(synonym);
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonym(Type synonymType)
        {
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            // TODO: check whether this even exists...
            var synonym = new ReflectionSynonym(this, Dialect, synonymType);
            return OptionAsync<IDatabaseSynonym>.Some(synonym);
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseSynonym> LoadSynonyms()
        {
            var synonyms = TypeProvider.Synonyms.Select(LoadSynonymSync).ToList();
            if (synonyms.Empty())
                return EmptySynonymLookup;

            var (duplicateNames, lookup) = CreateLookup(synonyms);
            if (duplicateNames.Any())
            {
                var message = "Duplicates found for the following synonyms: " + duplicateNames.Join(", ");
                throw new Exception(message);
            }

            return lookup;
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            routineName = CreateQualifiedIdentifier(routineName);

            return Routine.TryGetValue(routineName, out var routine)
                ? OptionAsync<IDatabaseRoutine>.Some(routine)
                : OptionAsync<IDatabaseRoutine>.None;
        }

        public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default) => Routine.Values
            .OrderBy(r => r.Name.Schema)
            .ThenBy(r => r.Name.LocalName)
            .ToAsyncEnumerable();

        protected IReadOnlyDictionary<Identifier, IDatabaseRoutine> Routine => _routineLookup.Value;

        protected virtual Option<IDatabaseRoutine> LoadRoutineSync(Type routineType)
        {
            if (routineType == null)
                throw new ArgumentNullException(nameof(routineType));

            return Option<IDatabaseRoutine>.None;
        }

        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Type routineType)
        {
            if (routineType == null)
                throw new ArgumentNullException(nameof(routineType));

            return OptionAsync<IDatabaseRoutine>.None;
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRoutine> LoadRoutines() => EmptyRoutineLookup;

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? IdentifierDefaults.Server;
            var databaseName = identifier.Database ?? IdentifierDefaults.Database;
            var schema = identifier.Schema ?? IdentifierDefaults.Schema;

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
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseView>> _viewLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSequence>> _sequenceLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseSynonym>> _synonymLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseRoutine>> _routineLookup;

        private static readonly IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> EmptyTableLookup = new Dictionary<Identifier, IRelationalDatabaseTable>();
        private static readonly IReadOnlyDictionary<Identifier, IDatabaseView> EmptyViewLookup = new Dictionary<Identifier, IDatabaseView>();
        private static readonly IReadOnlyDictionary<Identifier, IDatabaseSequence> EmptySequenceLookup = new Dictionary<Identifier, IDatabaseSequence>();
        private static readonly IReadOnlyDictionary<Identifier, IDatabaseSynonym> EmptySynonymLookup = new Dictionary<Identifier, IDatabaseSynonym>();
        private static readonly IReadOnlyDictionary<Identifier, IDatabaseRoutine> EmptyRoutineLookup = new Dictionary<Identifier, IDatabaseRoutine>();
    }
}
