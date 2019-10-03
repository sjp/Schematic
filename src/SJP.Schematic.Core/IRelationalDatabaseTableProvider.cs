using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTableProvider
    {
        OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default);
    }
}
