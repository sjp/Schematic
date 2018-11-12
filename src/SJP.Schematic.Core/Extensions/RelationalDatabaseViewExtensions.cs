using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions
{
    public static class RelationalDatabaseViewExtensions
    {
        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columns = view.Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns)
            {
                if (column.Name != null)
                    result[column.Name.LocalName] = column;
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetColumnLookup(view);
            return new IdentifierResolvingDictionary<IDatabaseColumn>(lookup, identifierResolver);
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(this IRelationalDatabaseView view, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return GetColumnLookupAsyncCore(view, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsyncCore(IRelationalDatabaseView view, CancellationToken cancellationToken)
        {
            var columns = await view.ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns)
            {
                if (column.Name != null)
                    result[column.Name.LocalName] = column;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(this IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetColumnLookupAsyncCore(view, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsyncCore(IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetColumnLookupAsync(view, cancellationToken).ConfigureAwait(false);
            return new IdentifierResolvingDictionary<IDatabaseColumn>(lookup, identifierResolver);
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetIndexLookup(this IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var indexes = view.Indexes;
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
            {
                if (index.Name != null)
                    result[index.Name.LocalName] = index;
            }

            return result;
        }

        public static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetIndexLookup(this IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetIndexLookup(view);
            return new IdentifierResolvingDictionary<IDatabaseIndex>(lookup, identifierResolver);
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsync(this IRelationalDatabaseView view, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return GetIndexLookupAsyncCore(view, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsyncCore(IRelationalDatabaseView view, CancellationToken cancellationToken)
        {
            var indexes = await view.IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
            {
                if (index.Name != null)
                    result[index.Name.LocalName] = index;
            }

            return result;
        }

        public static Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsync(this IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            return GetIndexLookupAsyncCore(view, identifierResolver, cancellationToken);
        }

        private static async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> GetIndexLookupAsyncCore(IRelationalDatabaseView view, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
        {
            var lookup = await GetIndexLookupAsync(view, cancellationToken).ConfigureAwait(false);
            return new IdentifierResolvingDictionary<IDatabaseIndex>(lookup, identifierResolver);
        }
    }
}
