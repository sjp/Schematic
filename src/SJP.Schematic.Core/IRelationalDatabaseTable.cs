using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable
    {
        IDatabaseKey PrimaryKey { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }
    }
}
