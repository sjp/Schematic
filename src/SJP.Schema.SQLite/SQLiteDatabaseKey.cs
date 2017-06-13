using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite
{
    public class SqliteDatabaseKey : IDatabaseKey
    {
        public SqliteDatabaseKey(IRelationalDatabaseTable table, Identifier name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Name = name; // can be null!
            Table = table ?? throw new ArgumentNullException(nameof(table)); ;
            KeyType = keyType;
            Columns = columns;
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public DatabaseKeyType KeyType { get; }

        public IEnumerable<IDatabaseColumn> Columns { get; }
    }
}
