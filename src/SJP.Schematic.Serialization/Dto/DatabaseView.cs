using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class DatabaseView
{
    public Identifier ViewName { get; set; } = default!;

    public string Definition { get; set; } = default!;

    public IEnumerable<DatabaseColumn> Columns { get; set; } = Array.Empty<DatabaseColumn>();

    public bool IsMaterialized { get; set; }
}