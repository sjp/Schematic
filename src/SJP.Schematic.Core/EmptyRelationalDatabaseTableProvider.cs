using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; } = Array.Empty<IRelationalDatabaseTable>();

        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyTables;

        private readonly static Task<IReadOnlyCollection<IRelationalDatabaseTable>> _emptyTables = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>(Array.Empty<IRelationalDatabaseTable>());

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Option<IRelationalDatabaseTable>.None;
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return OptionAsync<IRelationalDatabaseTable>.None;
        }
    }
}
