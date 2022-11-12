using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseViewComments
{
    public required Identifier ViewName { get; init; }

    public string? Comment { get; init; }

    public required IReadOnlyDictionary<string, string?> ColumnComments { get; init; }
}