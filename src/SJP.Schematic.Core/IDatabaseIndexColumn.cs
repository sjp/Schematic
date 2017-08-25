using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseIndexColumn
    {
        IndexColumnOrder Order { get; }

        IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        string GetExpression(IDatabaseDialect dialect);
    }
}
