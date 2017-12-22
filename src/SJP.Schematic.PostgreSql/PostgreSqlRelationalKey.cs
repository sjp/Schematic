using System;
using System.Data;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalKey : IDatabaseRelationalKey
    {
        public PostgreSqlRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, Rule deleteRule, Rule updateRule)
        {
            if (!deleteRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));
            if (!updateRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(updateRule));

            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

            if (ChildKey.KeyType != DatabaseKeyType.Foreign)
                throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{ childKey.KeyType.ToString() }'.", nameof(childKey));
            if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{ parentKey.KeyType.ToString() }'.", nameof(parentKey));

            DeleteRule = deleteRule;
            UpdateRule = updateRule;
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public Rule DeleteRule { get; }

        public Rule UpdateRule { get; }
    }
}
