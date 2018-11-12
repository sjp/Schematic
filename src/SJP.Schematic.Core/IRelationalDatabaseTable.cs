using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable
    {
        // sync
        IDatabaseKey PrimaryKey { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        // async
        Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
