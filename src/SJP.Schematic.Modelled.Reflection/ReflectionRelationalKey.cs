using System;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionRelationalKey : IDatabaseRelationalKey
    {
        public ReflectionRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, RelationalKeyUpdateAction deleteAction, RelationalKeyUpdateAction updateAction)
        {
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            DeleteAction = deleteAction;
            UpdateAction = updateAction;

            ValidateColumnSetsCompatible(childKey, parentKey);
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public RelationalKeyUpdateAction DeleteAction { get; }

        public RelationalKeyUpdateAction UpdateAction { get; }

        private static void ValidateColumnSetsCompatible(IDatabaseKey childKey, IDatabaseKey parentKey)
        {
            var anyComputed = childKey.Columns.Any(c => c.IsComputed) || parentKey.Columns.Any(c => c.IsComputed);
            if (anyComputed)
                return;

            var childKeyColumnTypes = childKey.Columns.Select(c => c.Type);
            var parentKeyColumnTypes = parentKey.Columns.Select(c => c.Type);

            var compatible = childKeyColumnTypes
                .Zip(parentKeyColumnTypes, (a, b) => new { Column = a, TargetColumn = b })
                .All(cc => IsTypeEquivalent(cc.Column, cc.TargetColumn));

            if (!compatible)
                throw new Exception($"The column sets in the foreign key { childKey.Name } from { childKey.Table.Name } to { parentKey.Name } in { parentKey.Table.Name } are not compatible. Please check the declarations to ensure the column types are safe to create.");
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
