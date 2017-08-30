using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalKey : IDatabaseRelationalKey
    {
        public SqliteRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, RelationalKeyUpdateAction deleteAction, RelationalKeyUpdateAction updateAction)
        {
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The { nameof(RelationalKeyUpdateAction) } provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The { nameof(RelationalKeyUpdateAction) } provided must be a valid enum.", nameof(updateAction));

            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            DeleteAction = deleteAction;
            UpdateAction = updateAction;
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public RelationalKeyUpdateAction DeleteAction { get; }

        public RelationalKeyUpdateAction UpdateAction { get; }
    }
}
