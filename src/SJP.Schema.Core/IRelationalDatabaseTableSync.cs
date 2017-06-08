using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseTableSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IDatabaseKey PrimaryKey { get; }

        IReadOnlyDictionary<string, IDatabaseTableColumn> Column { get; }

        IList<IDatabaseTableColumn> Columns { get; }

        IReadOnlyDictionary<string, IDatabaseCheckConstraint> CheckConstraint { get; }

        IEnumerable<IDatabaseCheckConstraint> CheckConstraints { get; }

        IReadOnlyDictionary<string, IDatabaseTableIndex> Index { get; }

        IEnumerable<IDatabaseTableIndex> Indexes { get; }

        IReadOnlyDictionary<string, IDatabaseKey> UniqueKey { get; }

        IEnumerable<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyDictionary<string, IDatabaseRelationalKey> ParentKey { get; }

        IEnumerable<IDatabaseRelationalKey> ParentKeys { get; }

        IEnumerable<IDatabaseRelationalKey> ChildKeys { get; }

        // TRIGGER ON TABLE or DATABASE OR BOTH?
        IReadOnlyDictionary<string, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }
    }
}
