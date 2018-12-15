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

            var tableOption = database.TryGetTableAsync(tableName, CancellationToken.None).GetAwaiter().GetResult();
            table = tableOption.table;

            return tableOption.exists;
        }

        public static bool TryGetView(this IRelationalDatabase database, Identifier viewName, out IRelationalDatabaseView view)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var viewOption = database.TryGetViewAsync(viewName, CancellationToken.None).GetAwaiter().GetResult();
            view = viewOption.view;

            return viewOption.exists;
        }

        public static bool TryGetSequence(this IRelationalDatabase database, Identifier sequenceName, out IDatabaseSequence sequence)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var sequenceOption = database.TryGetSequenceAsync(sequenceName, CancellationToken.None).GetAwaiter().GetResult();
            sequence = sequenceOption.sequence;

            return sequenceOption.exists;
        }

        public static bool TryGetSynonym(this IRelationalDatabase database, Identifier synonymName, out IDatabaseSynonym synonym)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var synonymOption = database.TryGetSynonymAsync(synonymName, CancellationToken.None).GetAwaiter().GetResult();
            synonym = synonymOption.synonym;

            return synonymOption.exists;
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
            var tableOption = database.GetTable(tableName, cancellationToken);
            var exists = await tableOption.IsSome.ConfigureAwait(false);
            var table = await tableOption.IfNoneUnsafe(default(IRelationalDatabaseTable)).ConfigureAwait(false);

            return (exists, table);
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
            var viewOption = database.GetView(viewName, cancellationToken);
            var exists = await viewOption.IsSome.ConfigureAwait(false);
            var view = await viewOption.IfNoneUnsafe(default(IRelationalDatabaseView)).ConfigureAwait(false);

            return (exists, view);
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
            var sequenceOption = database.GetSequence(sequenceName, cancellationToken);
            var exists = await sequenceOption.IsSome.ConfigureAwait(false);
            var sequence = await sequenceOption.IfNoneUnsafe(default(IDatabaseSequence)).ConfigureAwait(false);

            return (exists, sequence);
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
            var synonymOption = database.GetSynonym(synonymName, cancellationToken);
            var exists = await synonymOption.IsSome.ConfigureAwait(false);
            var synonym = await synonymOption.IfNoneUnsafe(default(IDatabaseSynonym)).ConfigureAwait(false);

            return (exists, synonym);
        }
    }
}
