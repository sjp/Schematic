using System;
using System.Data;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalKey : IDatabaseRelationalKey
    {
        public OracleRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, Rule deleteRule)
        {
            if (!deleteRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));

            ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

            if (ChildKey.KeyType != DatabaseKeyType.Foreign)
                throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{ childKey.KeyType.ToString() }'.", nameof(childKey));
            if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{ parentKey.KeyType.ToString() }'.", nameof(parentKey));

            DeleteRule = deleteRule;
        }

        public Identifier ChildTable { get; }

        public IDatabaseKey ChildKey { get; }

        public Identifier ParentTable { get; }

        public IDatabaseKey ParentKey { get; }

        public Rule DeleteRule { get; }

        public Rule UpdateRule { get; }
    }
}
