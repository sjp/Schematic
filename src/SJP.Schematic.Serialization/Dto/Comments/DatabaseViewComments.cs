using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments
{
    public class DatabaseViewComments
    {
        public Identifier ViewName { get; set; } = default!;

        public string? Comment { get; set; }

        public IReadOnlyDictionary<string, string?> ColumnComments { get; set; } = new Dictionary<string, string?>();
    }
}
