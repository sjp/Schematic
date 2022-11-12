using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseIndexColumn
{
    public required Core.IndexColumnOrder Order { get; init; }

    public required IEnumerable<DatabaseColumn> DependentColumns { get; init; }

    public required string? Expression { get; init; }
}