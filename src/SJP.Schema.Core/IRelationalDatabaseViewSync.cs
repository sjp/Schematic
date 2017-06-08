using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseViewSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IReadOnlyDictionary<string, IDatabaseViewColumn> Column { get; }

        IList<IDatabaseViewColumn> Columns { get; }

        IReadOnlyDictionary<string, IDatabaseViewIndex> Index { get; }

        IEnumerable<IDatabaseViewIndex> Indexes { get; }
    }
}
