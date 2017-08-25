using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseViewAsync : IDatabaseEntity
    {
        Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync();

        Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync();

        Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync();
    }
}
