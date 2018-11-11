using System;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    public static class RelationalDatabaseExtensions
    {
        public static bool TryGetTable(this IRelationalDatabase database, Identifier tableName, out IRelationalDatabaseTable table)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var tableOption = database.GetTable(tableName);
            table = tableOption.IfNoneUnsafe((IRelationalDatabaseTable)null);

            return tableOption.IsSome;
        }

        public static bool TryGetView(this IRelationalDatabase database, Identifier viewName, out IRelationalDatabaseView view)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewOption = database.GetView(viewName);
            view = viewOption.IfNoneUnsafe((IRelationalDatabaseView)null);

            return viewOption.IsSome;
        }

        public static bool TryGetSequence(this IRelationalDatabase database, Identifier sequenceName, out IDatabaseSequence sequence)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequenceOption = database.GetSequence(sequenceName);
            sequence = sequenceOption.IfNoneUnsafe((IDatabaseSequence)null);

            return sequenceOption.IsSome;
        }

        public static bool TryGetSynonym(this IRelationalDatabase database, Identifier synonymName, out IDatabaseSynonym synonym)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymOption = database.GetSynonym(synonymName);
            synonym = synonymOption.IfNoneUnsafe((IDatabaseSynonym)null);

            return synonymOption.IsSome;
        }

        public static Task<(bool exists, IRelationalDatabaseTable table)> TryGetTableAsync(this IRelationalDatabase database, Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TryGetTableAsyncCore(database, tableName, cancellationToken);
        }

        private static async Task<(bool exists, IRelationalDatabaseTable table)> TryGetTableAsyncCore(IRelationalDatabase database, Identifier tableName, CancellationToken cancellationToken)
        {
            var tableOption = await database.GetTableAsync(tableName, cancellationToken).ConfigureAwait(false);
            var exists = tableOption.IsSome;

            return (exists, tableOption.IfNoneUnsafe((IRelationalDatabaseTable)null));
        }

        public static Task<(bool exists, IRelationalDatabaseView view)> TryGetViewAsync(this IRelationalDatabase database, Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return TryGetViewAsyncCore(database, viewName, cancellationToken);
        }

        private static async Task<(bool exists, IRelationalDatabaseView view)> TryGetViewAsyncCore(IRelationalDatabase database, Identifier viewName, CancellationToken cancellationToken)
        {
            var viewOption = await database.GetViewAsync(viewName, cancellationToken).ConfigureAwait(false);
            var exists = viewOption.IsSome;

            return (exists, viewOption.IfNoneUnsafe((IRelationalDatabaseView)null));
        }

        public static Task<(bool exists, IDatabaseSequence sequence)> TryGetSequenceAsync(this IRelationalDatabase database, Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return TryGetSequenceAsyncCore(database, sequenceName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseSequence sequence)> TryGetSequenceAsyncCore(IRelationalDatabase database, Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequenceOption = await database.GetSequenceAsync(sequenceName, cancellationToken).ConfigureAwait(false);
            var exists = sequenceOption.IsSome;

            return (exists, sequenceOption.IfNoneUnsafe((IDatabaseSequence)null));
        }

        public static Task<(bool exists, IDatabaseSynonym synonym)> TryGetSynonymAsync(this IRelationalDatabase database, Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return TryGetSynonymAsyncCore(database, synonymName, cancellationToken);
        }

        private static async Task<(bool exists, IDatabaseSynonym synonym)> TryGetSynonymAsyncCore(IRelationalDatabase database, Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonymOption = await database.GetSynonymAsync(synonymName, cancellationToken).ConfigureAwait(false);
            var exists = synonymOption.IsSome;

            return (exists, synonymOption.IfNoneUnsafe((IDatabaseSynonym)null));
        }
    }
}
