using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseRelationalKey
{
    public Identifier? ChildTable { get; set; }

    public DatabaseKey? ChildKey { get; set; }

    public Identifier? ParentTable { get; set; }

    public DatabaseKey? ParentKey { get; set; }

    public ReferentialAction DeleteAction { get; set; }

    public ReferentialAction UpdateAction { get; set; }
}