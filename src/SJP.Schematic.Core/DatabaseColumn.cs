using System;

namespace SJP.Schematic.Core
{
    public abstract class DatabaseColumn : IDatabaseColumn
    {
        protected DatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName.LocalName;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
            AutoIncrement = autoIncrement;
        }

        public string DefaultValue { get; }

        public virtual bool IsComputed { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public IAutoIncrement AutoIncrement { get; }
    }

    public class DatabaseTableColumn : DatabaseColumn, IDatabaseTableColumn
    {
        public DatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class DatabaseViewColumn : DatabaseColumn, IDatabaseViewColumn
    {
        public DatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
