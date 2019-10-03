using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IRelationalDatabaseTable>();

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return OptionAsync<IRelationalDatabaseTable>.None;
        }
    }
}
