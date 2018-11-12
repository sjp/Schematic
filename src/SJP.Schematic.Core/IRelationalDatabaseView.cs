using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable
    {
        // sync
        string Definition { get; }

        IReadOnlyList<IDatabaseColumn> Columns { get; }

        IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        bool IsIndexed { get; }

        // async
        Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
