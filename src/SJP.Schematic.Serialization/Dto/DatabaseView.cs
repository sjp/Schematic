using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseView
    {
        public Identifier? Name { get; set; }

        public string? Definition { get; set; }

        public IEnumerable<DatabaseColumn> Columns { get; set; } = Array.Empty<DatabaseColumn>();

        public bool IsMaterialized { get; set; }
    }
}
