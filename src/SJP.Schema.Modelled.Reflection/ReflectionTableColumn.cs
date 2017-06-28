using System;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTableColumn : IDatabaseTableColumn
    {
        public ReflectionTableColumn(IDatabaseDialect dialect, IRelationalDatabaseTable table, PropertyInfo prop, Type declaredColumnType, bool isNullable)
        {
            Property = prop ?? throw new ArgumentNullException(nameof(prop));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            if (declaredColumnType == null)
                throw new ArgumentNullException(nameof(declaredColumnType));

            Name = dialect.GetNameOverrideOrDefault(prop);
            var clrType = GetClrType(declaredColumnType);
            if (clrType == null)
                throw new ArgumentNullException($"The declared column type does not implement IDbType<T>. Check { prop.DeclaringType.FullName }.{ prop.Name } and ensure that the column type { declaredColumnType.FullName } implements this interface.", nameof(declaredColumnType));
            var columnType = new ReflectionColumnDataType(dialect, declaredColumnType, clrType);

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Type = columnType ?? throw new ArgumentNullException(nameof(declaredColumnType));
            IsNullable = isNullable;

        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        public IDbType Type { get; }

        public string DefaultValue { get; }

        // TODO: get from property
        public bool IsAutoIncrement { get; }

        public bool IsCalculated { get; }

        protected IDatabaseDialect Dialect { get; }

        protected PropertyInfo Property { get; }

        protected static Type GetClrType(Type columnType)
        {
            if (columnType == null)
                throw new ArgumentNullException(nameof(columnType));

            var dbTypeInterface = columnType.GetTypeInfo().GetInterfaces().SingleOrDefault(iface => ModelledTypeInterface.GetTypeInfo().GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(iface));
            if (dbTypeInterface == null)
                return null;

            return dbTypeInterface.GetTypeInfo().GetGenericArguments().Single();
        }

        protected static Type ModelledTypeInterface { get; } = typeof(IDbType<>);
    }
}
