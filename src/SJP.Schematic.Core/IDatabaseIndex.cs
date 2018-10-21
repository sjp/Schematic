using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseIndex : IDatabaseOptional
    {
        Identifier Name { get; }

        IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

        bool IsUnique { get; }
    }
}
