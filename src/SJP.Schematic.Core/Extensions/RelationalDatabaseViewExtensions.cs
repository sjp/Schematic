using System;
using System.Collections.Generic;
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
    }
}
