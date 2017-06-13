using System;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite
{
    public class SqliteRelationalKey : IDatabaseRelationalKey
    {
        public SqliteRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey)
        {
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }
    }
}
