using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    // do we need async key interfaces?
    public interface IDatabaseKeyAsync
    {
        IRelationalDatabaseTable Table { get; }

        Identifier Name { get; }

        Task<IEnumerable<IDatabaseColumn>> ColumnsAsync();
        // maybe add is primary?
        // maybe add is unique?
        // do we even care?
    }
}
