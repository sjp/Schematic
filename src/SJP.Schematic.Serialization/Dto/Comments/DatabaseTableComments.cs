using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseTableComments
{
    public required Identifier TableName { get; init; }

    public string? Comment { get; init; }

    public string? PrimaryKeyComment { get; init; }

    public required IReadOnlyDictionary<string, string?> ColumnComments { get; init; }

    public required IReadOnlyDictionary<string, string?> CheckComments { get; init; }

    public required IReadOnlyDictionary<string, string?> UniqueKeyComments { get; init; }

    public required IReadOnlyDictionary<string, string?> ForeignKeyComments { get; init; }

    public required IReadOnlyDictionary<string, string?> IndexComments { get; init; }

    public required IReadOnlyDictionary<string, string?> TriggerComments { get; init; }
}