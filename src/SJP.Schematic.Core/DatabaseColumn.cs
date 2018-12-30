using System;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public class DatabaseColumn : IDatabaseColumn
    {
        public DatabaseColumn(
            Identifier columnName,
            IDbType type,
            bool isNullable,
            Option<string> defaultValue,
            Option<IAutoIncrement> autoIncrement
        )
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName.LocalName;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
            AutoIncrement = autoIncrement;
        }

        public Option<string> DefaultValue { get; }

        public virtual bool IsComputed { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public Option<IAutoIncrement> AutoIncrement { get; }
    }
}
