using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IDatabaseViewStatistic : IDatabaseStatistic<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }

        new IEnumerable<IDatabaseViewColumn> Columns { get; }
    }
}
