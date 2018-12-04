using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable
    {
        Option<IDatabaseKey> PrimaryKey { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }
    }
}
