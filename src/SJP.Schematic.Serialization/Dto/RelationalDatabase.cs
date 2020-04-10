using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto
{
    public class RelationalDatabase
    {
        public IdentifierDefaults IdentifierDefaults { get; set; } = default!;

        public IIdentifierResolutionStrategy? IdentifierResolver { get; set; }

        public IEnumerable<RelationalDatabaseTable> Tables { get; set; } = Array.Empty<RelationalDatabaseTable>();

        public IEnumerable<DatabaseView> Views { get; set; } = Array.Empty<DatabaseView>();

        public IEnumerable<DatabaseSequence> Sequences { get; set; } = Array.Empty<DatabaseSequence>();

        public IEnumerable<DatabaseSynonym> Synonyms { get; set; } = Array.Empty<DatabaseSynonym>();

        public IEnumerable<DatabaseRoutine> Routines { get; set; } = Array.Empty<DatabaseRoutine>();
    }
}
