using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions
{
    /// <summary>
    /// Convenience extension methods used for working with <see cref="IDatabaseView"/> instances.
    /// </summary>
    public static class DatabaseViewExtensions
    {
        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IDatabaseView view)
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

        public static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(this IDatabaseView view, IIdentifierResolutionStrategy identifierResolver)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var lookup = GetColumnLookup(view);
            return new IdentifierResolvingDictionary<IDatabaseColumn>(lookup, identifierResolver);
        }
    }
}
