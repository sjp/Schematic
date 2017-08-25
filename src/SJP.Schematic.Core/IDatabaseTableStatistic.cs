using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseTableStatistic : IDatabaseStatistic<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }

        new IEnumerable<IDatabaseTableColumn> Columns { get; }
    }
}
