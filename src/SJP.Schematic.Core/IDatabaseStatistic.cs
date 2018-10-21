using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseStatistic : IDatabaseEntity
    {
        IReadOnlyCollection<IDatabaseColumn> Columns { get; }
    }
}
