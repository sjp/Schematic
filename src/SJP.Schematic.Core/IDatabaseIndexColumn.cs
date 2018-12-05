using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseIndexColumn
    {
        IndexColumnOrder Order { get; }

        string Expression { get; }

        IReadOnlyList<IDatabaseColumn> DependentColumns { get; }
    }
}
