using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseColumn : IDatabaseColumn
    {
        public MySqlDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
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
}
