using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization
{
    internal class RelationalDatabase : IRelationalDatabase
    {
        public IDatabaseDialect Dialect { get; set; }

        public IIdentifierDefaults IdentifierDefaults { get; set; }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get => _tables;
            set => _tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyCollection<IRelationalDatabaseTable> _tables = Array.Empty<IRelationalDatabaseTable>();

        public IReadOnlyCollection<IDatabaseView> Views
        {
            get => _views;
            set => _views = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyCollection<IDatabaseView> _views = Array.Empty<IDatabaseView>();

        public IReadOnlyCollection<IDatabaseSequence> Sequences
        {
            get => _sequences;
            set => _sequences = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyCollection<IDatabaseSequence> _sequences = Array.Empty<IDatabaseSequence>();

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms
        {
            get => _synonyms;
            set => _synonyms = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyCollection<IDatabaseSynonym> _synonyms = Array.Empty<IDatabaseSynonym>();

        public IReadOnlyCollection<IDatabaseRoutine> Routines
        {
            get => _routines;
            set => _routines = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IReadOnlyCollection<IDatabaseRoutine> _routines = Array.Empty<IDatabaseRoutine>();

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var sequence = Sequences.FirstOrDefault(s => sequenceName == s.Name);
            return sequence != null
                ? OptionAsync<IDatabaseSequence>.Some(sequence)
                : OptionAsync<IDatabaseSequence>.None;
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var synonym = Synonyms.FirstOrDefault(s => synonymName == s.Name);
            return synonym != null
                ? OptionAsync<IDatabaseSynonym>.Some(synonym)
                : OptionAsync<IDatabaseSynonym>.None;
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var table = Tables.FirstOrDefault(t => tableName == t.Name);
            return table != null
                ? OptionAsync<IRelationalDatabaseTable>.Some(table)
                : OptionAsync<IRelationalDatabaseTable>.None;
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var view = Views.FirstOrDefault(v => viewName == v.Name);
            return view != null
                ? OptionAsync<IDatabaseView>.Some(view)
                : OptionAsync<IDatabaseView>.None;
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            routineName = CreateQualifiedIdentifier(routineName);
            var routine = Routines.FirstOrDefault(r => routineName == r.Name);
            return routine != null
                ? OptionAsync<IDatabaseRoutine>.Some(routine)
                : OptionAsync<IDatabaseRoutine>.None;
        }

        public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Sequences);
        }

        public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Synonyms);
        }

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Tables);
        }

        public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Views);
        }

        public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Routines);
        }

        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? IdentifierDefaults?.Server;
            var databaseName = identifier.Database ?? IdentifierDefaults?.Database;
            var schema = identifier.Schema ?? IdentifierDefaults?.Schema;
            var localName = identifier.LocalName;

            return Identifier.CreateQualifiedIdentifier(serverName, databaseName, schema, localName);
        }
    }
}
