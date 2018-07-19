using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable
    {
        // sync
        IRelationalDatabase Database { get; }

        string Definition { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column { get; }

        IReadOnlyList<IDatabaseViewColumn> Columns { get; }

        IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index { get; }

        IReadOnlyCollection<IDatabaseViewIndex> Indexes { get; }

        bool IsIndexed { get; }

        // async
        Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
