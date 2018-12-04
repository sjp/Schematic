using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable
    {
        string Definition { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        bool IsIndexed { get; }
    }
}
