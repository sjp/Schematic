using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable
    {
        // sync
        IDatabaseKey PrimaryKey { get; }

        IReadOnlyDictionary<Identifier, IDatabaseColumn> Column { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check { get; }

        IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        IReadOnlyDictionary<Identifier, IDatabaseIndex> Index { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey { get; }

        IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        // async
        Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
