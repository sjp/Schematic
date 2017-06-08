using System;
using SJP.Schema.Core;

namespace SJP.Schema.SQLite
{
    public abstract class SQLiteDatabaseColumn : IDatabaseColumn
    {
        protected SQLiteDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Name = columnName;
            Type = type;
            IsNullable = isNullable;
            DefaultValue = defaultValue;
            IsAutoIncrement = isAutoIncrement;
        }

        public string DefaultValue { get; }

        public virtual bool IsCalculated { get; } = false;

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public bool IsAutoIncrement { get; }
    }

    public class SQLiteDatabaseTableColumn : SQLiteDatabaseColumn, IDatabaseTableColumn
    {
        public SQLiteDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class SQLiteDatabaseViewColumn : SQLiteDatabaseColumn, IDatabaseViewColumn
    {
        public SQLiteDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

}
