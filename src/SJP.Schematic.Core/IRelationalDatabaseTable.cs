using System.Collections.Generic;
using System.Threading;
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
        Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
