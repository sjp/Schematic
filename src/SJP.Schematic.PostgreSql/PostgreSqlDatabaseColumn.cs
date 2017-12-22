using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public abstract class PostgreSqlDatabaseColumn : IDatabaseColumn
    {
        protected PostgreSqlDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
        {
            if (columnName == null || columnName.LocalName == null)
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

    public class PostgreSqlDatabaseTableColumn : PostgreSqlDatabaseColumn, IDatabaseTableColumn
    {
        public PostgreSqlDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class PostgreSqlDatabaseViewColumn : PostgreSqlDatabaseColumn, IDatabaseViewColumn
    {
        public PostgreSqlDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
