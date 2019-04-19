using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class RelationalDatabaseTable
    {
        public Identifier Name { get; set; }

        public DatabaseKey PrimaryKey { get; set; }

        public IEnumerable<DatabaseColumn> Columns { get; set; }

        public IEnumerable<DatabaseCheckConstraint> Checks { get; set; }

        public IEnumerable<DatabaseIndex> Indexes { get; set; }

        public IEnumerable<DatabaseKey> UniqueKeys { get; set; }

        public IEnumerable<DatabaseRelationalKey> ParentKeys { get; set; }

        public IEnumerable<DatabaseRelationalKey> ChildKeys { get; set; }

        public IEnumerable<DatabaseTrigger> Triggers { get; set; }
    }
}
