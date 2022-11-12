using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseView
{
    public required Identifier ViewName { get; init; }

    public required string Definition { get; init; }

    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    public required bool IsMaterialized { get; init; }
}