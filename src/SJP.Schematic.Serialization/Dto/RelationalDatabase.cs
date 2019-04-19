using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class RelationalDatabase
    {
        public IEnumerable<RelationalDatabaseTable> Tables { get; set; }

        public IEnumerable<DatabaseView> Views { get; set; }

        public IEnumerable<DatabaseSequence> Sequences { get; set; }

        public IEnumerable<DatabaseSynonym> Synonyms { get; set; }

        public IEnumerable<DatabaseRoutine> Routines { get; set; }
    }
}
