using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseViewAsync : IDatabaseEntity
    {
        Task<IReadOnlyDictionary<string, IDatabaseViewColumn>> ColumnAsync();

        Task<IList<IDatabaseViewColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseViewIndex>> IndexAsync();

        Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync();
    }
}
