using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseViewStatistic : IDatabaseStatistic<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }

        new IEnumerable<IDatabaseViewColumn> Columns { get; }
    }
}
