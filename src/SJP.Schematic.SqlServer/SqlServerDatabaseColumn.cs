using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public abstract class SqlServerDatabaseColumn : IDatabaseColumn
    {
        protected SqlServerDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
        {
            if (columnName == null || columnName.LocalName == null)
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName.LocalName;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
            IsAutoIncrement = isAutoIncrement;
        }

        public string DefaultValue { get; }

        public virtual bool IsComputed { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public bool IsAutoIncrement { get; }
    }

    public class SqlServerDatabaseTableColumn : SqlServerDatabaseColumn, IDatabaseTableColumn
    {
        public SqlServerDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class SqlServerDatabaseViewColumn : SqlServerDatabaseColumn, IDatabaseViewColumn
    {
        public SqlServerDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

}
