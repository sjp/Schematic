using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseKey
{
    public Identifier? Name { get; init; }

    public required Core.DatabaseKeyType KeyType { get; init; }

    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    public required bool IsEnabled { get; init; }
}