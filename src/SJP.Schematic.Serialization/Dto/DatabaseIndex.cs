using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseIndex
    {
        public Identifier Name { get; set; }

        public IEnumerable<DatabaseIndexColumn> Columns { get; set; }

        public IEnumerable<DatabaseColumn> IncludedColumns { get; set; }

        public bool IsUnique { get; set; }

        public bool IsEnabled { get; set; }
    }
}