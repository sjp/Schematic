using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerRelationalKey : IDatabaseRelationalKey
    {
        public SqlServerRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, RelationalKeyUpdateAction deleteAction, RelationalKeyUpdateAction updateAction)
        {
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
