using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable
    {
        // sync
        IRelationalDatabase Database { get; }

        IDatabaseKey PrimaryKey { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column { get; }

        IReadOnlyList<IDatabaseTableColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check { get; }

        IEnumerable<IDatabaseCheckConstraint> Checks { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index { get; }

        IEnumerable<IDatabaseTableIndex> Indexes { get; }

        IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey { get; }

        IEnumerable<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey { get; }

        IEnumerable<IDatabaseRelationalKey> ParentKeys { get; }

        IEnumerable<IDatabaseRelationalKey> ChildKeys { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }

        // async
        Task<IDatabaseKey> PrimaryKeyAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync();
        Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync();
        Task<IEnumerable<IDatabaseCheckConstraint>> ChecksAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync();
        Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync();
        Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync();
        Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync();

        Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync();
        Task<IEnumerable<IDatabaseTrigger>> TriggersAsync();
    }
}
