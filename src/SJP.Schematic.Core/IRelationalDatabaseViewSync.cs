using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseViewSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        string Definition { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column { get; }

        IReadOnlyList<IDatabaseViewColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index { get; }

        IEnumerable<IDatabaseViewIndex> Indexes { get; }
    }
}
