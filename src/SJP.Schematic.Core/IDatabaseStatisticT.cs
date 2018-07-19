using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseStatistic<T> : IDatabaseEntity where T : IDatabaseQueryable
    {
        T Parent { get; }

        IReadOnlyCollection<IDatabaseColumn> Columns { get; }
    }
}
