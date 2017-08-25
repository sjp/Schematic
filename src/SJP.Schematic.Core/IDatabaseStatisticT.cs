using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseStatistic<T> : IDatabaseEntity where T : IDatabaseQueryable
    {
        T Parent { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }
    }
}
