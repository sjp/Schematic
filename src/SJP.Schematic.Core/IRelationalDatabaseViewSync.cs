using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseViewSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column { get; }

        IReadOnlyList<IDatabaseViewColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index { get; }

        IEnumerable<IDatabaseViewIndex> Indexes { get; }
    }
}
