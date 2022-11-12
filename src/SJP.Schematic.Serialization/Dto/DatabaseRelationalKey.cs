using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseRelationalKey
{
    public Identifier? ChildTable { get; init; }

    public DatabaseKey? ChildKey { get; init; }

    public Identifier? ParentTable { get; init; }

    public DatabaseKey? ParentKey { get; init; }

    public required ReferentialAction DeleteAction { get; init; }

    public required ReferentialAction UpdateAction { get; init; }
}