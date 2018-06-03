using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTableTypeProvider
    {
        public ReflectionTableTypeProvider(IDatabaseDialect dialect, Type tableType)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            TableType = tableType ?? throw new ArgumentNullException(nameof(tableType));
            if (tableType.IsAbstract)
                throw new ArgumentException($"The given table type '{ tableType.FullName }' is abstract. A non-abstract table type must be provided.", nameof(tableType));

            _columns = new Lazy<IEnumerable<IModelledColumn>>(LoadColumns);
            _checks = new Lazy<IEnumerable<IModelledCheckConstraint>>(LoadChecks);
            _primaryKey = new Lazy<IModelledKey>(LoadPrimaryKey);
            _uniqueKeys = new Lazy<IEnumerable<IModelledKey>>(LoadUniqueKeys);
            _parentKeys = new Lazy<IEnumerable<IModelledRelationalKey>>(LoadParentKeys);
            _indexes = new Lazy<IEnumerable<IModelledIndex>>(LoadIndexes);

            TableProperties = TableType.GetTypeInfo().GetProperties();
            TableInstance = CreateTableInstance();
        }

        public object TableInstance { get; }

        protected IDatabaseDialect Dialect { get; }

        protected Type TableType { get; }

        protected IEnumerable<PropertyInfo> TableProperties { get; }

        public IEnumerable<IModelledColumn> Columns => _columns.Value;

        public IEnumerable<IModelledCheckConstraint> Checks => _checks.Value;

        public IModelledKey PrimaryKey => _primaryKey.Value;

        public IEnumerable<IModelledKey> UniqueKeys => _uniqueKeys.Value;

        public IEnumerable<IModelledRelationalKey> ParentKeys => _parentKeys.Value;

        public IEnumerable<IModelledIndex> Indexes => _indexes.Value;

        protected object CreateTableInstance()
        {
            var ctor = TableType.GetDefaultConstructor();
            if (ctor == null)
                throw new ArgumentException($"The table type '{ TableType.FullName }' does not contain a default constructor.");

            var tableInstance = Activator.CreateInstance(TableType);
            return PopulateColumnProperties(tableInstance);
        }

        protected virtual object PopulateColumnProperties(object tableInstance)
        {
            if (tableInstance == null)
                throw new ArgumentNullException(nameof(tableInstance));

            var argType = tableInstance.GetType();
            if (argType != TableType)
                throw new ArgumentException($"The given table instance must match the type provider's table type. The type provider supports '{ TableType.FullName }', but was given an object of type '{ argType.FullName }'", nameof(tableInstance));

            var columnProps = TableProperties.Where(IsColumnProperty);
            foreach (var prop in columnProps)
            {
                var field = prop.GetAutoBackingField();
                if (field == null)
                    throw new ArgumentException($"The column property { TableType.FullName }.{ prop.Name } must be an auto-implemented property. For example: Column<T> { prop.Name } {{ get; }}");

                var columnDbType = field.FieldType;
                var columnPropertyProp = columnDbType.GetTypeInfo().GetProperty(nameof(IModelledColumn.Property), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var columnInstance = Activator.CreateInstance(columnDbType);
                columnPropertyProp.SetValue(columnInstance, prop);
                field.SetValue(tableInstance, columnInstance);
            }

            return tableInstance;
        }

        protected virtual IEnumerable<IModelledColumn> LoadColumns()
        {
            return TableProperties
                .Where(p => IsColumnProperty(p) || IsComputedColumnProperty(p))
                .Select(p => p.GetValue(TableInstance) as IModelledColumn)
                .ToList();
        }

        protected virtual IEnumerable<IModelledCheckConstraint> LoadChecks()
        {
            return TableProperties
                .Where(IsCheckProperty)
                .Select(p =>
                {
                    var check = p.GetValue(TableInstance) as IModelledCheckConstraint;
                    check.Property = p;
                    return check;
                })
                .ToList();
        }

        protected virtual IEnumerable<IModelledIndex> LoadIndexes()
        {
            return TableProperties
                .Where(IsIndexProperty)
                .Select(p =>
                {
                    var index = p.GetValue(TableInstance) as IModelledIndex;
                    index.Property = p;
                    return index;
                })
                .ToList();
        }

        protected virtual IModelledKey LoadPrimaryKey()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            var primaryKeys = new List<IModelledKey>();

            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as IModelledKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Primary)
                    primaryKeys.Add(keyValue);
            }

            if (primaryKeys.Count == 0)
                return null;
            if (primaryKeys.Count > 1)
                throw new ArgumentException("More than one primary key provided to " + TableType.FullName);

            return primaryKeys[0];
        }

        protected virtual IEnumerable<IModelledKey> LoadUniqueKeys()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Count == 0)
                return Array.Empty<IModelledKey>();

            var uniqueKeys = new List<IModelledKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as IModelledKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Unique)
                    uniqueKeys.Add(keyValue);
            }

            return uniqueKeys;
        }

        protected virtual IEnumerable<IModelledRelationalKey> LoadParentKeys()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Count == 0)
                return Array.Empty<IModelledRelationalKey>();

            var foreignKeys = new List<IModelledRelationalKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as IModelledRelationalKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type { typeof(IModelledRelationalKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Foreign)
                    foreignKeys.Add(keyValue);
            }

            return foreignKeys;
        }

        protected static bool IsColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsGenericType
            && prop.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(ColumnType.GetTypeInfo());

        protected static bool IsComputedColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(ComputedColumnType.GetTypeInfo());

        protected static bool IsIndexProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(IndexType.GetTypeInfo());

        protected static bool IsKeyProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(KeyType.GetTypeInfo());

        protected static bool IsCheckProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(CheckType.GetTypeInfo());

        protected static Type ColumnType { get; } = typeof(Column<>);

        protected static Type ComputedColumnType { get; } = typeof(ComputedColumn);

        protected static Type KeyType { get; } = typeof(Key);

        protected static Type CheckType { get; } = typeof(Check);

        protected static Type IndexType { get; } = typeof(Index);

        private readonly Lazy<IEnumerable<IModelledColumn>> _columns;
        private readonly Lazy<IEnumerable<IModelledCheckConstraint>> _checks;
        private readonly Lazy<IModelledKey> _primaryKey;
        private readonly Lazy<IEnumerable<IModelledKey>> _uniqueKeys;
        private readonly Lazy<IEnumerable<IModelledRelationalKey>> _parentKeys;
        private readonly Lazy<IEnumerable<IModelledIndex>> _indexes;
    }
}
