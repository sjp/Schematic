using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseIndex
{
    public Identifier? IndexName { get; set; } = default!;

    public IEnumerable<DatabaseIndexColumn> Columns { get; set; } = Array.Empty<DatabaseIndexColumn>();

    public IEnumerable<DatabaseColumn> IncludedColumns { get; set; } = Array.Empty<DatabaseColumn>();

    public bool IsUnique { get; set; }

    public bool IsEnabled { get; set; }
}