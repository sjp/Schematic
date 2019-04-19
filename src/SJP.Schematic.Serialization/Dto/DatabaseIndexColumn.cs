using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseIndexColumn
    {
        public Core.IndexColumnOrder Order { get; set; }

        public IEnumerable<DatabaseColumn> DependentColumns { get; set; }

        public string Expression { get; set; }
    }
}