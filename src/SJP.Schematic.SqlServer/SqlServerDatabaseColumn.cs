using System;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public abstract class SqlServerDatabaseColumn : IDatabaseColumn
    {
        protected SqlServerDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
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

    public class SqlServerDatabaseTableColumn : SqlServerDatabaseColumn, IDatabaseTableColumn
    {
        public SqlServerDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class SqlServerDatabaseViewColumn : SqlServerDatabaseColumn, IDatabaseViewColumn
    {
        public SqlServerDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
