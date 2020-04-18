using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class MySqlRelationalKey : IDatabaseRelationalKey
    {
        public MySqlRelationalKey(Identifier childTableName, IDatabaseKey childKey, Identifier parentTableName, IDatabaseKey parentKey, ReferentialAction deleteAction, ReferentialAction updateAction)
        {
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(updateAction));
            if (deleteAction == ReferentialAction.SetDefault)
                throw new ArgumentException("MySQL does not support a delete action of 'SET DEFAULT'.", nameof(deleteAction));
            if (updateAction == ReferentialAction.SetDefault)
                throw new ArgumentException("MySQL does not support an update action of 'SET DEFAULT'.", nameof(updateAction));

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Relational Key: ");

                if (!ChildTable.Schema.IsNullOrWhiteSpace())
                    builder.Append(ChildTable.Schema).Append('.');

                builder.Append(ChildTable.LocalName);

                ChildKey.Name.IfSome(name =>
                {
                    builder.Append(" (")
                        .Append(name.LocalName)
                        .Append(")");
                });

                builder.Append(" -> ");

                if (!ParentTable.Schema.IsNullOrWhiteSpace())
                    builder.Append(ParentTable.Schema).Append('.');

                builder.Append(ParentTable.LocalName);

                ParentKey.Name.IfSome(name =>
                {
                    builder.Append(" (")
                        .Append(name.LocalName)
                        .Append(")");
                });

                return builder.GetStringAndRelease();
            }
        }
    }
}
