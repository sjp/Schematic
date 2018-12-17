using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseView : IDatabaseQueryable
    {
        string Definition { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        bool IsMaterialized { get; }
    }
}
