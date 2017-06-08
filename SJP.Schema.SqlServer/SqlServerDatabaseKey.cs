using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseKey : IDatabaseKey
    {
        public SqlServerDatabaseKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Table = table;
            Name = name;
            KeyType = keyType;
            Columns = columns;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IEnumerable<IDatabaseColumn> Columns { get; }
    }
}
