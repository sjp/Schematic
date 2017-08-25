using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseIndex<T> : IDatabaseOptional where T : IDatabaseQueryable
    {
        T Parent { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseIndexColumn> Columns { get; }

        IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        bool IsUnique { get; }
    }
}
