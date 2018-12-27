﻿using System;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseColumn : IDatabaseColumn
    {
        public OracleDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue)
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

        public Option<IAutoIncrement> AutoIncrement { get; } = Option<IAutoIncrement>.None;
    }
}
