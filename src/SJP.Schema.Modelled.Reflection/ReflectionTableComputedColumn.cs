using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTableComputedColumn : IDatabaseTableColumn
    {
        public ReflectionTableComputedColumn(IDatabaseDialect dialect, IRelationalDatabaseTable table, PropertyInfo prop)
        {
            Property = prop ?? throw new ArgumentNullException(nameof(prop));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));

            Name = dialect.GetAliasOrDefault(prop);
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Type = new ReflectionComputedColumnDataType();
            IsNullable = true;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type { get; }

        public string DefaultValue { get; }

        public bool IsAutoIncrement { get; }

        public bool IsComputed { get; } = true;

        protected IDatabaseDialect Dialect { get; }

        protected PropertyInfo Property { get; }

        protected class ReflectionComputedColumnDataType : IDbType
        {
            public DataType Type { get; } = DataType.Unknown;

            public bool IsFixedLength { get; }

            public int Length => -1;

            public Type ClrType => typeof(object);
        }
    }
}
