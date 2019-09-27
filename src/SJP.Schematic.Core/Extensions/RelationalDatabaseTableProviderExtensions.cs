using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class RelationalDatabaseTableProviderExtensions
    {
        public static bool TryGetTable(this IRelationalDatabaseTableProvider tableProvider, Identifier tableName, [NotNullWhen(true)] out IRelationalDatabaseTable? table)
        {
            if (tableProvider == null)
                throw new ArgumentNullException(nameof(tableProvider));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableOption = TryGetTableAsyncCore(tableProvider, tableName, CancellationToken.None).GetAwaiter().GetResult();
            table = tableOption.table;

            return tableOption.exists;
        }

        public static Task<(bool exists, IRelationalDatabaseTable? table)> TryGetTableAsync(this IRelationalDatabaseTableProvider tableProvider, Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableProvider == null)
                throw new ArgumentNullException(nameof(tableProvider));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TryGetTableAsyncCore(tableProvider, tableName, cancellationToken);
        }

        private static async Task<(bool exists, IRelationalDatabaseTable? table)> TryGetTableAsyncCore(IRelationalDatabaseTableProvider tableProvider, Identifier tableName, CancellationToken cancellationToken)
        {
            var tableOption = tableProvider.GetTable(tableName, cancellationToken);
            var exists = await tableOption.IsSome.ConfigureAwait(false);
            var table = await tableOption.IfNoneUnsafe(default(IRelationalDatabaseTable)!).ConfigureAwait(false);

            return (exists, table);
        }
    }
}
