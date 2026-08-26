using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseRelationalKey
{
    public required Identifier ChildTable { get; init; }

    public required DatabaseKey ChildKey { get; init; }

    public required Identifier ParentTable { get; init; }

    public required DatabaseKey ParentKey { get; init; }

    public required ReferentialAction DeleteAction { get; init; }

    public required ReferentialAction UpdateAction { get; init; }
}