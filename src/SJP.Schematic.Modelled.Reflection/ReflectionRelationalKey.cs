using System;
using System.Data;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionRelationalKey : IDatabaseRelationalKey
    {
        public ReflectionRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, Rule deleteRule, Rule updateRule)
        {
            if (childKey == null)
                throw new ArgumentNullException(nameof(childKey));
            if (parentKey == null)
                throw new ArgumentNullException(nameof(parentKey));
            if (!deleteRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));
            if (!updateRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(updateRule));

            // perform validation
            var relationalKey = new DatabaseRelationalKey(childKey, parentKey, deleteRule, updateRule);

            ChildKey = relationalKey.ChildKey;
            ParentKey = relationalKey.ParentKey;
            DeleteRule = relationalKey.DeleteRule;
            UpdateRule = relationalKey.UpdateRule;

            ValidateColumnSetsCompatible(ChildKey, ParentKey);
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public Rule DeleteRule { get; }

        public Rule UpdateRule { get; }

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
