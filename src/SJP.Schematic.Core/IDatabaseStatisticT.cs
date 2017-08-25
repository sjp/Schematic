using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseStatistic<T> : IDatabaseEntity where T : IDatabaseQueryable
    {
        T Parent { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }
    }
}
