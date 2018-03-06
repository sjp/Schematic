using System.Collections.Generic;
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

        IEnumerable<IDatabaseViewIndex> Indexes { get; }

        bool IsIndexed { get; }

        // async
        Task<string> DefinitionAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync();

        Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync();

        Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync();
    }
}
