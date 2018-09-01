using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public abstract class OracleDatabaseColumn : IDatabaseColumn
    {
        protected OracleDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName.LocalName;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
        }

        public string DefaultValue { get; }

        public virtual bool IsComputed { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public IAutoIncrement AutoIncrement { get; }
    }

    public class OracleDatabaseTableColumn : OracleDatabaseColumn, IDatabaseTableColumn
    {
        public OracleDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue)
            : base(columnName, type, isNullable, defaultValue)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class OracleDatabaseViewColumn : OracleDatabaseColumn, IDatabaseViewColumn
    {
        public OracleDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue)
            : base(columnName, type, isNullable, defaultValue)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
