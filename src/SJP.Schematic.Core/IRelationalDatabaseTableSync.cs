using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTableSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IDatabaseKey PrimaryKey { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column { get; }

        IReadOnlyList<IDatabaseTableColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint { get; }

        IEnumerable<IDatabaseCheckConstraint> CheckConstraints { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index { get; }

        IEnumerable<IDatabaseTableIndex> Indexes { get; }

        IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey { get; }

        IEnumerable<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey { get; }

        IEnumerable<IDatabaseRelationalKey> ParentKeys { get; }

        IEnumerable<IDatabaseRelationalKey> ChildKeys { get; }

        // TRIGGER ON TABLE or DATABASE OR BOTH?
        IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }
    }
}
