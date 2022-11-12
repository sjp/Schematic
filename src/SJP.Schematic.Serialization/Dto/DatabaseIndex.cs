using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseIndex
{
    public Identifier? IndexName { get; init; }

    public required IEnumerable<DatabaseIndexColumn> Columns { get; init; }

    public required IEnumerable<DatabaseColumn> IncludedColumns { get; init; }

    public required bool IsUnique { get; init; }

    public required bool IsEnabled { get; init; }

    public string? FilterDefinition { get; init; }
}