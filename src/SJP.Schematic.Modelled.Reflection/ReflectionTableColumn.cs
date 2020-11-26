using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTableColumn : IDatabaseColumn
    {
        public ReflectionTableColumn(IDatabaseDialect dialect, PropertyInfo prop, Type declaredColumnType, bool isNullable)
        {
            Property = prop ?? throw new ArgumentNullException(nameof(prop));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            if (declaredColumnType == null)
                throw new ArgumentNullException(nameof(declaredColumnType));

            Name = dialect.GetAliasOrDefault(prop);
            var clrType = GetClrType(declaredColumnType);
            if (clrType == null)
                throw new ArgumentNullException($"The declared column type does not implement IDbType<T>. Check { prop.ReflectedType!.FullName }.{ prop.Name } and ensure that the column type { declaredColumnType.FullName } implements this interface.", nameof(declaredColumnType));

            var columnType = new ReflectionColumnDataType(dialect, declaredColumnType, clrType);
            var autoIncrAttr = dialect.GetDialectAttribute<AutoIncrementAttribute>(declaredColumnType)
                ?? dialect.GetDialectAttribute<AutoIncrementAttribute>(prop);

            if (autoIncrAttr != null)
            {
                if (!ValidAutoIncrementTypes.Contains(columnType.DataType))
                    throw new ArgumentNullException($"The column { prop.ReflectedType!.FullName }.{ prop.Name } is declared as being auto incrementing, which is not supported on a '{ columnType.DataType }' data type.", nameof(declaredColumnType));

                AutoIncrement = new AutoIncrement(autoIncrAttr.InitialValue, autoIncrAttr.Increment);
            }

            Type = columnType;
            IsNullable = isNullable;
        }

        public bool IsNullable { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public Option<string> DefaultValue { get; }

        public Option<IAutoIncrement> AutoIncrement { get; }

        public bool IsComputed { get; }

        protected IDatabaseDialect Dialect { get; }

        protected PropertyInfo Property { get; }

        protected static Type GetClrType(Type columnType)
        {
            if (columnType == null)
                throw new ArgumentNullException(nameof(columnType));

            var columnTypeInfo = columnType.GetTypeInfo();
            var dbTypeInterface = columnTypeInfo.ImplementedInterfaces
                .SingleOrDefault(static iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == ModelledTypeInterface);

            return dbTypeInterface?.GetTypeInfo()?.GetGenericArguments()?.Single() ?? typeof(object);
        }

        protected static Type ModelledTypeInterface { get; } = typeof(IDbType<>);

        protected static IEnumerable<DataType> ValidAutoIncrementTypes { get; } = new System.Collections.Generic.HashSet<DataType>
        {
            DataType.BigInteger,
            DataType.Integer,
            DataType.SmallInteger,
            DataType.Numeric
        };
    }
}
