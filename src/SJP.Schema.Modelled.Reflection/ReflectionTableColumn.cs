using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTableColumn : IDatabaseTableColumn
    {
        public ReflectionTableColumn(IRelationalDatabaseTable table, PropertyInfo prop, IDbType type, bool isNullable)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));
            Name = prop.Name; // TODO: this should be based on dialect or injected...

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            Property = prop;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type { get; }

        public string DefaultValue { get; } = null;

        // TODO: get from property
        public bool IsAutoIncrement { get; } = false;

        public bool IsCalculated { get; } = false;

        protected PropertyInfo Property { get; }
    }
}
