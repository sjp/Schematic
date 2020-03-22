using System;
using EnumsNET;

namespace SJP.Schematic.Core
{
    public class DatabaseRelationalKey : IDatabaseRelationalKey
    {
        public DatabaseRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, ReferentialAction deleteAction, ReferentialAction updateAction)
        {
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(updateAction));

            ChildTable = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentTable = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));

            if (ChildKey.KeyType != DatabaseKeyType.Foreign)
                throw new ArgumentException($"The child key must be a foreign key, instead given a key of type '{ childKey.KeyType }'.", nameof(childKey));
            if (ParentKey.KeyType != DatabaseKeyType.Primary && ParentKey.KeyType != DatabaseKeyType.Unique)
                throw new ArgumentException($"The parent key must be a primary or unique key, instead given a key of type '{ parentKey.KeyType }'.", nameof(parentKey));

            DeleteAction = deleteAction;
            UpdateAction = updateAction;
        }

        public Identifier ChildTable { get; }

        public IDatabaseKey ChildKey { get; }

        public Identifier ParentTable { get; }

        public IDatabaseKey ParentKey { get; }

        public ReferentialAction DeleteAction { get; }

        public ReferentialAction UpdateAction { get; }
    }
}
