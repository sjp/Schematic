using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    // TODO: fix this so that we can point to synonyms
    public class ReflectionForeignKey : ReflectionKey
    {
        public ReflectionForeignKey(IRelationalDatabaseTable table, IDatabaseKey targetKey, PropertyInfo property, IEnumerable<IDatabaseColumn> columns)
            : base(table, property, columns, DatabaseKeyType.Foreign)
        {
            if (targetKey == null)
                throw new ArgumentNullException(nameof(targetKey));
            if (targetKey.KeyType != DatabaseKeyType.Primary && targetKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException("The parent key given to a foreign key must be a primary or unique key. Instead given: " + targetKey.KeyType.ToString(), nameof(targetKey));

            if (columns.Count() != targetKey.Columns.Count())
                throw new ArgumentException("The number of columns given to a foreign key must match the number of columns in the target key", nameof(columns));

            var columnTypes = columns.Select(c => c.Type).ToList();
            var targetColumnTypes = columns.Select(c => c.Type).ToList();
            if (!ColumnTypesCompatible(columnTypes, targetColumnTypes))
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
