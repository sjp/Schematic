using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    /// <summary>
    /// TODO: Refactor to avoid keeping all objects in memory in lookups.
    /// </summary>
    internal sealed class SynonymTargets
    {
        public SynonymTargets(
            IEnumerable<IRelationalDatabaseTable> tables,
            IEnumerable<IDatabaseView> views,
            IEnumerable<IDatabaseSequence> sequences,
            IEnumerable<IDatabaseSynonym> synonyms,
            IEnumerable<IDatabaseRoutine> routines
        )
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            var tableLookup = new Dictionary<Identifier, IRelationalDatabaseTable>();
            foreach (var table in tables)
                tableLookup[table.Name] = table;

            var viewLookup = new Dictionary<Identifier, IDatabaseView>();
            foreach (var view in views)
                viewLookup[view.Name] = view;

            var sequenceLookup = new Dictionary<Identifier, IDatabaseSequence>();
            foreach (var sequence in sequences)
                sequenceLookup[sequence.Name] = sequence;

            var synonymLookup = new Dictionary<Identifier, IDatabaseSynonym>();
            foreach (var synonym in synonyms)
                synonymLookup[synonym.Name] = synonym;

            var routineLookup = new Dictionary<Identifier, IDatabaseRoutine>();
            foreach (var routine in routines)
                routineLookup[routine.Name] = routine;

            Tables = tableLookup;
            Views = viewLookup;
            Sequences = sequenceLookup;
            Synonyms = synonymLookup;
            Routines = routineLookup;
        }

        public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Tables { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseView> Views { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequences { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonyms { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseRoutine> Routines { get; }
    }
}
