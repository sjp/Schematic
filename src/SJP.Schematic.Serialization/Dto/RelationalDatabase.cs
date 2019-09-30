using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class RelationalDatabase
    {
        public IEnumerable<RelationalDatabaseTable> Tables { get; set; } = Array.Empty<RelationalDatabaseTable>();

        public IEnumerable<DatabaseView> Views { get; set; } = Array.Empty<DatabaseView>();

        public IEnumerable<DatabaseSequence> Sequences { get; set; } = Array.Empty<DatabaseSequence>();

        public IEnumerable<DatabaseSynonym> Synonyms { get; set; } = Array.Empty<DatabaseSynonym>();

        public IEnumerable<DatabaseRoutine> Routines { get; set; } = Array.Empty<DatabaseRoutine>();
    }
}
