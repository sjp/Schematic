using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments
{
    public class DatabaseTableComments
    {
        public Identifier TableName { get; set; } = default!;

        public string? Comment { get; set; }

        public string? PrimaryKeyComment { get; set; }

        public IReadOnlyDictionary<string, string?> ColumnComments { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, string?> CheckComments { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, string?> UniqueKeyComments { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, string?> ForeignKeyComments { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, string?> IndexComments { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, string?> TriggerComments { get; set; } = new Dictionary<string, string?>();
    }
}
