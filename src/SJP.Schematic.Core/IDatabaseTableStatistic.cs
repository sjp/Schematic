using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseTableStatistic : IDatabaseStatistic<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }

        new IEnumerable<IDatabaseTableColumn> Columns { get; }
    }
}
