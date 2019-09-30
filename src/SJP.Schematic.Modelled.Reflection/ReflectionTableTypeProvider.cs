using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
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

            _columns = new Lazy<IReadOnlyCollection<IModelledColumn>>(LoadColumns);
            _checks = new Lazy<IReadOnlyCollection<IModelledCheckConstraint>>(LoadChecks);
            _primaryKey = new Lazy<IModelledKey?>(LoadPrimaryKey);
            _uniqueKeys = new Lazy<IReadOnlyCollection<IModelledKey>>(LoadUniqueKeys);
            _parentKeys = new Lazy<IReadOnlyCollection<IModelledRelationalKey>>(LoadParentKeys);
            _indexes = new Lazy<IReadOnlyCollection<IModelledIndex>>(LoadIndexes);

            TableProperties = TableType.GetProperties();
            TableInstance = CreateTableInstance();
        }

        public object TableInstance { get; }

        protected IDatabaseDialect Dialect { get; }

        protected Type TableType { get; }

        protected IReadOnlyCollection<PropertyInfo> TableProperties { get; }

        public IReadOnlyCollection<IModelledColumn> Columns => _columns.Value;

        public IReadOnlyCollection<IModelledCheckConstraint> Checks => _checks.Value;

        public IModelledKey? PrimaryKey => _primaryKey.Value;

        public IReadOnlyCollection<IModelledKey> UniqueKeys => _uniqueKeys.Value;

        public IReadOnlyCollection<IModelledRelationalKey> ParentKeys => _parentKeys.Value;

        public IReadOnlyCollection<IModelledIndex> Indexes => _indexes.Value;

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
                var columnInstance = Activator.CreateInstance(columnDbType);
                field.SetValue(tableInstance, columnInstance);
            }

            return tableInstance;
        }

        protected virtual IReadOnlyCollection<IModelledColumn> LoadColumns()
        {
            return TableProperties
                .Where(p => (IsColumnProperty(p) || IsComputedColumnProperty(p)) && p.GetValue(TableInstance) is IModelledColumn)
                .Select(c =>
                {
                    var column = c.GetValue(TableInstance) as IModelledColumn;
                    column!.Property = c;
                    return column;
                })
                .ToList();
        }

        protected virtual IReadOnlyCollection<IModelledCheckConstraint> LoadChecks()
        {
            return TableProperties
                .Where(IsCheckProperty)
                .Where(p => p.GetValue(TableInstance) is IModelledCheckConstraint)
                .Select(p =>
                {
                    var check = p.GetValue(TableInstance) as IModelledCheckConstraint;
                    check!.Property = p;
                    return check;
                })
                .ToList();
        }

        protected virtual IReadOnlyCollection<IModelledIndex> LoadIndexes()
        {
            return TableProperties
                .Where(IsIndexProperty)
                .Where(p => p.GetValue(TableInstance) is IModelledIndex)
                .Select(p =>
                {
                    var index = p.GetValue(TableInstance) as IModelledIndex;
                    index!.Property = p;
                    return index;
                })
                .ToList();
        }

        protected virtual IModelledKey? LoadPrimaryKey()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            var primaryKeys = new List<IModelledKey>();

            foreach (var keyProperty in keyProperties)
            {
                if (!(keyProperty.GetValue(TableInstance) is IModelledKey keyValue))
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Primary)
                    primaryKeys.Add(keyValue);
            }

            if (primaryKeys.Empty())
                return null;
            if (primaryKeys.Count > 1)
                throw new ArgumentException("More than one primary key provided to " + TableType.FullName);

            return primaryKeys[0];
        }

        protected virtual IReadOnlyCollection<IModelledKey> LoadUniqueKeys()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Empty())
                return Array.Empty<IModelledKey>();

            var uniqueKeys = new List<IModelledKey>();
            foreach (var keyProperty in keyProperties)
            {
                if (!(keyProperty.GetValue(TableInstance) is IModelledKey keyValue))
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Unique)
                    uniqueKeys.Add(keyValue);
            }

            return uniqueKeys;
        }

        protected virtual IReadOnlyCollection<IModelledRelationalKey> LoadParentKeys()
        {
            var keyProperties = TableProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Empty())
                return Array.Empty<IModelledRelationalKey>();

            var foreignKeys = new List<IModelledRelationalKey>();
            foreach (var keyProperty in keyProperties)
            {
                if (!(keyProperty.GetValue(TableInstance) is IModelledRelationalKey keyValue))
                    throw new InvalidCastException($"Expected to find a key type { typeof(IModelledRelationalKey).FullName } on { TableType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Foreign)
                    foreignKeys.Add(keyValue);
            }

            return foreignKeys;
        }

        protected static bool IsColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.IsGenericType
            && prop.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(ColumnType);

        protected static bool IsComputedColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.IsAssignableFrom(ComputedColumnType);

        protected static bool IsIndexProperty(PropertyInfo prop) =>
            prop.PropertyType.IsAssignableFrom(IndexType);

        protected static bool IsKeyProperty(PropertyInfo prop) =>
            prop.PropertyType.IsAssignableFrom(KeyType);

        protected static bool IsCheckProperty(PropertyInfo prop) =>
            prop.PropertyType.IsAssignableFrom(CheckType);

        protected static Type ColumnType { get; } = typeof(Column<>);

        protected static Type ComputedColumnType { get; } = typeof(ComputedColumn);

        protected static Type KeyType { get; } = typeof(Key);

        protected static Type CheckType { get; } = typeof(Check);

        protected static Type IndexType { get; } = typeof(Model.Index);

        private readonly Lazy<IReadOnlyCollection<IModelledColumn>> _columns;
        private readonly Lazy<IReadOnlyCollection<IModelledCheckConstraint>> _checks;
        private readonly Lazy<IModelledKey?> _primaryKey;
        private readonly Lazy<IReadOnlyCollection<IModelledKey>> _uniqueKeys;
        private readonly Lazy<IReadOnlyCollection<IModelledRelationalKey>> _parentKeys;
        private readonly Lazy<IReadOnlyCollection<IModelledIndex>> _indexes;
    }
}
