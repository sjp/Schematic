using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    // TODO: fix this so that we can point to synonyms
    public class ReflectionForeignKey : ReflectionKey
    {
        public ReflectionForeignKey(IRelationalDatabaseTable table, Identifier name, IDatabaseKey targetKey, IEnumerable<IDatabaseColumn> columns)
            : base(table, name, DatabaseKeyType.Foreign, columns)
        {
            if (targetKey == null)
                throw new ArgumentNullException(nameof(targetKey));
            if (targetKey.KeyType != DatabaseKeyType.Primary && targetKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException("The parent key given to a foreign key must be a primary or unique key. Instead given: " + targetKey.KeyType.ToString(), nameof(targetKey));
            if (columns.Count() != targetKey.Columns.Count())
                throw new ArgumentException("The number of columns given to a foreign key must match the number of columns in the target key", nameof(columns));

            var columnTypes = columns.Select(c => c.Type).ToList();
            var targetColumnTypes = targetKey.Columns.Select(c => c.Type).ToList();

            // if we're dealing with computed columns, we can't get the types easily so avoid checking the types
            var anyComputed = columns.Any(c => c.IsComputed) || targetKey.Columns.Any(c => c.IsComputed);
            var columnTypesCompatible = ColumnTypesCompatible(columnTypes, targetColumnTypes);

            if (!anyComputed && !columnTypesCompatible)
                throw new ArgumentException("Incompatible column types between source and target key columns.", nameof(columns));
        }

        private static bool ColumnTypesCompatible(IEnumerable<IDbType> columnTypes, IEnumerable<IDbType> targetTypes)
        {
            return columnTypes
                .Zip(targetTypes, (a, b) => new { Column = a, TargetColumn = b })
                .All(cc => IsTypeEquivalent(cc.Column, cc.TargetColumn));
        }

        private static bool IsTypeEquivalent(IDbType columnType, IDbType targetType)
        {
            return columnType.ClrType == targetType.ClrType
                && (columnType.IsFixedLength == targetType.IsFixedLength || (!columnType.IsFixedLength && targetType.IsFixedLength))
                && columnType.Length >= targetType.Length
                && columnType.Type == targetType.Type;
        }
    }
}
