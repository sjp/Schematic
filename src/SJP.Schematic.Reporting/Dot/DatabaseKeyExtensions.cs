using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Dot
{
    internal static class DatabaseKeyExtensions
    {
        public static int GetKeyHash(this IDatabaseKey key, Identifier tableName)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + tableName.GetHashCode();
                hash = (hash * 23) + key.KeyType.GetHashCode();
                foreach (var column in key.Columns)
                    hash = (hash * 23) + (column.Name?.LocalName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
