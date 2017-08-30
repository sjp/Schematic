using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionKey : IDatabaseKey
    {
        public ReflectionKey(IDatabaseDialect dialect, IRelationalDatabaseTable table, PropertyInfo prop, IEnumerable<IDatabaseColumn> columns, DatabaseKeyType keyType)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            _prop = prop ?? throw new ArgumentNullException(nameof(prop));
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Columns = columns.ToList();
            KeyType = keyType;

            Name = dialect.GetAliasOrDefault(prop);
        }

        public IEnumerable<IDatabaseColumn> Columns { get; }

        public DatabaseKeyType KeyType { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        // this should always be true
        // is there a situation where would not want it to be true?
        public bool IsEnabled { get; } = true;

        private readonly PropertyInfo _prop;
    }
}
