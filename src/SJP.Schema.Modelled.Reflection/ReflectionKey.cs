using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionKey : IDatabaseKey
    {
        public ReflectionKey(IRelationalDatabaseTable table, PropertyInfo prop, IEnumerable<IDatabaseColumn> columns, DatabaseKeyType keyType)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            _prop = prop ?? throw new ArgumentNullException(nameof(prop));
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Columns = columns.ToImmutableList();
            KeyType = keyType;

            Name = prop.Name; // TODO: FIX THIS!
        }

        public IEnumerable<IDatabaseColumn> Columns { get; }

        public DatabaseKeyType KeyType { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        private readonly PropertyInfo _prop;
    }
}
