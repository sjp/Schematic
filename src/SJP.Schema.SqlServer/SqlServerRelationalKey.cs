using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerRelationalKey : IDatabaseRelationalKey
    {
        public SqlServerRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey)
        {
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }
    }
}
