using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public abstract class SqliteDatabaseColumn : IDatabaseColumn
    {
        protected SqliteDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
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

    public class SqliteDatabaseTableColumn : SqliteDatabaseColumn, IDatabaseTableColumn
    {
        public SqliteDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class SqliteDatabaseViewColumn : SqliteDatabaseColumn, IDatabaseViewColumn
    {
        public SqliteDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

}
