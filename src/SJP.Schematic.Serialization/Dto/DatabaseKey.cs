using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseKey
    {
        public Identifier Name { get; set; }

        public Core.DatabaseKeyType KeyType { get; set; }

        public IEnumerable<DatabaseColumn> Columns { get; set; }

        public bool IsEnabled { get; set; }
    }
}