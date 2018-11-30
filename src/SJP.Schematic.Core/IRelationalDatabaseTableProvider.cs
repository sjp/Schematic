using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTableProvider
    {
        Option<IRelationalDatabaseTable> GetTable(Identifier tableName);

        OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
