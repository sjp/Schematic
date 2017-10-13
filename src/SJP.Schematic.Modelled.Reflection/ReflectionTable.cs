using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTable : IRelationalDatabaseTable
    {
        public ReflectionTable(IRelationalDatabase database, Type tableType)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            InstanceType = tableType ?? throw new ArgumentNullException(nameof(tableType));
            TableInstance = CreateTableInstance(InstanceType);
            InstanceProperties = new HashSet<PropertyInfo>(InstanceType.GetTypeInfo().GetProperties());

            _columns = new Lazy<IReadOnlyList<IDatabaseTableColumn>>(LoadColumnList);
            _columnLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>>(LoadColumns);
            _checkLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>>(LoadChecks);
            _uniqueKeyLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>>(LoadUniqueKeys);
            _indexLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>>(LoadIndexes);
            _parentKeyLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>>(LoadParentKeys);
            _childKeys = new Lazy<IEnumerable<IDatabaseRelationalKey>>(LoadChildKeys);
            _primaryKey = new Lazy<IDatabaseKey>(LoadPrimaryKey);

            Dialect = Database.Dialect;
            Name = Dialect.GetQualifiedNameOrDefault(database, InstanceType);
        }

        protected IDatabaseDialect Dialect { get; }

        private IEnumerable<IDatabaseRelationalKey> LoadChildKeys()
        {
            return Database.Tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ParentKey.Table.Name == Name)
                .ToList();
        }

        protected static object CreateTableInstance(Type tableType)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));

            var ctor = tableType.GetDefaultConstructor();
            if (ctor == null)
                throw new ArgumentException($"The provided table type '{ tableType.FullName }' does not contain a default constructor.", nameof(tableType));

            var tableInstance = Activator.CreateInstance(tableType);
            return PopulateColumnProperties(tableType, tableInstance);
        }

        protected static object PopulateColumnProperties(Type tableType, object tableInstance)
        {
            if (tableType == null)
                throw new ArgumentNullException(nameof(tableType));
            if (tableInstance == null)
                throw new ArgumentNullException(nameof(tableInstance));

            var columnProps = tableType
                .GetTypeInfo()
                .GetProperties()
                .Where(IsColumnProperty);

            foreach (var prop in columnProps)
            {
                var field = prop.GetAutoBackingField();
                if (field == null)
                    throw new ArgumentException($"The column property { tableType.FullName }.{ prop.Name } must be an auto-implemented property. For example: Column<T> { prop.Name } {{ get; }}");

                var columnDbType = field.FieldType;
                var columnPropertyProp = columnDbType.GetTypeInfo().GetProperty(nameof(IModelledColumn.Property), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var columnInstance = Activator.CreateInstance(columnDbType);
                columnPropertyProp.SetValue(columnInstance, prop);
                field.SetValue(tableInstance, columnInstance);
            }

            return tableInstance;
        }

        // TODO: see if it's possible to create a foreign key to a synonym in sql server
        //       this should be possible in oracle but not sure
        private IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeys()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>();
            var tableColumns = Column; // trigger load

            var keyProperties = InstanceProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Count == 0)
                return result;

            var fks = new List<Key.ForeignKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as Key.ForeignKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type { typeof(Key.ForeignKey).FullName } on { InstanceType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Foreign)
                    fks.Add(keyValue);
            }

            if (fks.Count == 0)
                return result;

            foreach (var fk in fks)
            {
                var fkColumns = fk.Columns
                    .Select(c => c.Property)
                    .Select(GetAnyColumnFromProperty);

                var parentName = Dialect.GetQualifiedNameOrDefault(Database, fk.TargetType);
                var parent = Database.GetTable(parentName);

                var parentTable = new ReflectionTable(Database, fk.TargetType);
                var parentInstance = parentTable.TableInstance;
                var keyObject = fk.KeySelector(parentInstance);
                var parentKeyName = Dialect.GetAliasOrDefault(keyObject.Property);

                IDatabaseKey parentKey;
                if (keyObject.KeyType == DatabaseKeyType.Primary)
                {
                    parentKey = parent.PrimaryKey;
                    if (parentKey == null)
                        throw new Exception("Could not find matching parent key for foreign key."); // TODO: provide better error messaging, maybe to KeyNotFoundException too?
                }
                else if (keyObject.KeyType == DatabaseKeyType.Unique)
                {
                    if (!parent.UniqueKey.ContainsKey(parentKeyName))
                        throw new Exception("Could not find matching parent key for foreign key."); // same goes for this...

                    parentKey = parent.UniqueKey[parentKeyName];
                }
                else
                {
                    throw new Exception("Cannot point a foreign key to a key that is not the primary key or a unique key."); // TODO: improve error messaging
                }

                // check that columns match up with parent key -- otherwise will fail
                // TODO: don't assume that the FK is to a table -- could be to a synonym
                //       maybe change interface of Synonym<T> to be something like Synonym<Table<T>> or Synonym<Synonym<T>> -- could unwrap at runtime?
                var childKey = new ReflectionForeignKey(Dialect, this, parentKey, fk.Property, fkColumns);

                var deleteAttr = Dialect.GetDialectAttribute<OnDeleteRuleAttributeAttribute>(fk.Property);
                var deleteRule = deleteAttr?.Rule ?? Rule.None;

                var updateAttr = Dialect.GetDialectAttribute<OnUpdateRuleAttribute>(fk.Property);
                var updateRule = updateAttr?.Rule ?? Rule.None;

                result[childKey.Name.LocalName] = new ReflectionRelationalKey(childKey, parentKey, deleteRule, updateRule);
            }

            return result.AsReadOnlyDictionary();
        }

        public object TableInstance { get; }

        private IReadOnlyList<IDatabaseTableColumn> LoadColumnList()
        {
            var result = new List<IDatabaseTableColumn>();

            foreach (var prop in InstanceProperties)
            {
                if (IsColumnProperty(prop))
                {
                    var column = GetColumnFromProperty(prop);
                    result.Add(column);
                }

                if (IsComputedColumnProperty(prop))
                {
                    var computedColumn = GetComputedColumnFromProperty(prop);
                    result.Add(computedColumn);
                }
            }

            return result.AsReadOnly();
        }

        protected Type InstanceType { get; }

        protected IEnumerable<PropertyInfo> InstanceProperties { get; }

        private IDatabaseKey LoadPrimaryKey()
        {
            var keyProperties = InstanceProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Count == 0)
                return null;

            var primaryKeys = new List<IModelledKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as IModelledKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { InstanceType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Primary)
                    primaryKeys.Add(keyValue);
            }

            if (primaryKeys.Count == 0)
                return null;
            if (primaryKeys.Count > 1)
                throw new ArgumentException("More than one primary key provided to " + InstanceType.FullName);

            var dialect = Database.Dialect;
            var primaryKey = primaryKeys.Single();
            var pkColumns = primaryKey.Columns
                .Select(c => c.Property)
                .Select(GetAnyColumnFromProperty);

            return new ReflectionKey(Dialect, this, primaryKey.Property, pkColumns, primaryKey.KeyType);
        }

        private IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexes()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>();

            var indexes = InstanceProperties
                .Where(IsIndexProperty)
                .Select(prop =>
                {
                    var index = prop.GetValue(TableInstance) as IModelledIndex;
                    index.Property = prop;
                    return index;
                });

            foreach (var index in indexes)
            {
                var refIndex = new ReflectionDatabaseTableIndex(this, index);
                result[refIndex.Name.LocalName] = refIndex;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeys()
        {
            var result = new Dictionary<Identifier, IDatabaseKey>();
            var tableColumns = Column; // trigger load

            var keyProperties = InstanceProperties.Where(IsKeyProperty).ToList();
            if (keyProperties.Count == 0)
                return result;

            var uniqueKeys = new List<IModelledKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as IModelledKey;
                if (keyValue == null)
                    throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { InstanceType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Unique)
                    uniqueKeys.Add(keyValue);
            }

            if (uniqueKeys.Count == 0)
                return result;

            foreach (var uniqueKey in uniqueKeys)
            {
                var ukColumns = uniqueKey.Columns
                    .Select(c => c.Property)
                    .Select(GetAnyColumnFromProperty);

                var uk = new ReflectionKey(Dialect, this, uniqueKey.Property, ukColumns, uniqueKey.KeyType);
                result[uk.Name.LocalName] = uk;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadChecks()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>();

            var dialect = Database.Dialect;
            var checkProperties = InstanceProperties.Where(IsCheckProperty);
            var columnPropertyNames = PropertyColumnCache.Keys
                .Select(prop => new KeyValuePair<string, PropertyInfo>(prop.Name, prop))
                .ToDictionary();

            foreach (var checkProperty in checkProperties)
            {
                var modelledCheck = checkProperty.GetValue(TableInstance) as IModelledCheckConstraint;
                modelledCheck.Property = checkProperty;
                var definition = modelledCheck.Expression.ToSql(dialect);

                var checkName = dialect.GetAliasOrDefault(checkProperty);
                var check = new ReflectionCheckConstraint(this, checkName, definition);
                result[check.Name.LocalName] = check;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseTableColumn> LoadColumns()
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>();

            foreach (var prop in InstanceProperties)
            {
                if (IsColumnProperty(prop))
                {
                    var column = GetColumnFromProperty(prop);
                    result[column.Name.LocalName] = column;
                }

                if (IsComputedColumnProperty(prop))
                {
                    var computedColumn = GetComputedColumnFromProperty(prop);
                    result[computedColumn.Name.LocalName] = computedColumn;
                }
            }

            return result.AsReadOnlyDictionary();
        }

        protected virtual IDatabaseTableColumn GetAnyColumnFromProperty(PropertyInfo propInfo)
        {
            if (propInfo == null)
                throw new ArgumentNullException(nameof(propInfo));

            if (IsColumnProperty(propInfo))
                return GetColumnFromProperty(propInfo);
            if (IsComputedColumnProperty(propInfo))
                return GetComputedColumnFromProperty(propInfo);

            return null;
        }

        protected virtual IDatabaseTableColumn GetColumnFromProperty(PropertyInfo propInfo)
        {
            if (propInfo == null)
                throw new ArgumentNullException(nameof(propInfo));
            if (PropertyColumnCache.ContainsKey(propInfo))
                return PropertyColumnCache[propInfo];

            if (!propInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(InstanceType.GetTypeInfo()))
                throw new ArgumentException($"The property { propInfo.Name } must be a member of { InstanceType.FullName }, instead is a member of { propInfo.DeclaringType.FullName }", nameof(propInfo));
            if (!IsColumnProperty(propInfo))
                throw new ArgumentException($"The property { InstanceType.FullName }.{ propInfo.Name } must be a column property (i.e. declared as Column<T>). Instead it is declared as { propInfo.DeclaringType.FullName }", nameof(propInfo));

            var modelledColumn = propInfo.GetValue(TableInstance) as IModelledColumn;
            var column = new ReflectionTableColumn(Dialect, this, modelledColumn.Property, modelledColumn.DeclaredDbType, modelledColumn.IsNullable);
            PropertyColumnCache[propInfo] = column; // add to cache
            return column;
        }

        protected virtual IDatabaseTableColumn GetComputedColumnFromProperty(PropertyInfo propInfo)
        {
            if (propInfo == null)
                throw new ArgumentNullException(nameof(propInfo));
            if (PropertyColumnCache.ContainsKey(propInfo))
                return PropertyColumnCache[propInfo];

            if (!IsComputedColumnProperty(propInfo))
                throw new ArgumentException($"The property { InstanceType.FullName }.{ propInfo.Name } must be a computed column property (i.e. declared as ComputedColumn). Instead it is declared as { propInfo.DeclaringType.FullName }", nameof(propInfo));

            var modelledColumn = propInfo.GetValue(TableInstance) as IModelledComputedColumn;
            var definition = modelledColumn.Expression.ToSql(Dialect);
            var column = new ReflectionTableComputedColumn(Dialect, this, propInfo, definition);
            PropertyColumnCache[propInfo] = column; // add to cache
            return column;
        }

        private static bool IsColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsGenericType
            && prop.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(ColumnType.GetTypeInfo());

        private static bool IsComputedColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(ComputedColumnType.GetTypeInfo());

        private static bool IsIndexProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(IndexType.GetTypeInfo());

        private static bool IsKeyProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(KeyType.GetTypeInfo());

        private static bool IsCheckProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(CheckType.GetTypeInfo());

        private static Type ColumnType { get; } = typeof(Column<>);

        private static Type ComputedColumnType { get; } = typeof(ComputedColumn);

        private static Type KeyType { get; } = typeof(Key);

        private static Type CheckType { get; } = typeof(Check);

        private static Type IndexType { get; } = typeof(Index);

        private static Type DbTypeArg { get; } = typeof(IDbType);

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint => _checkLookup.Value;

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => _checkLookup.Value.Values;

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => _childKeys.Value;

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column => _columnLookup.Value;

        public IReadOnlyList<IDatabaseTableColumn> Columns => _columns.Value;

        public IRelationalDatabase Database { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => _indexLookup.Value;

        public IEnumerable<IDatabaseTableIndex> Indexes => _indexLookup.Value.Values;

        public Identifier Name { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => _parentKeyLookup.Value;

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => _parentKeyLookup.Value.Values;

        public IDatabaseKey PrimaryKey => _primaryKey.Value;

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IEnumerable<IDatabaseTrigger> Triggers { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => _uniqueKeyLookup.Value;

        public IEnumerable<IDatabaseKey> UniqueKeys => _uniqueKeyLookup.Value.Values;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckConstraintAsync() => Task.FromResult(_checkLookup.Value);

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => Task.FromResult(_checkLookup.Value.Values);

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => Task.FromResult(_childKeys.Value);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync() => Task.FromResult(_columnLookup.Value);

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync()
        {
            var result = _columnLookup.Value.Values.ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<IDatabaseTableColumn>>(result);
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync() => Task.FromResult(_indexLookup.Value);

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => Task.FromResult(_indexLookup.Value.Values);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync() => Task.FromResult(_parentKeyLookup.Value);

        public Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync() => Task.FromResult(_parentKeyLookup.Value.Values);

        public Task<IDatabaseKey> PrimaryKeyAsync() => Task.FromResult(_primaryKey.Value);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDatabaseTrigger>> TriggersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync() => Task.FromResult(_uniqueKeyLookup.Value);

        public Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync() => Task.FromResult(_uniqueKeyLookup.Value.Values);

        protected IDictionary<PropertyInfo, IDatabaseTableColumn> PropertyColumnCache { get; } = new Dictionary<PropertyInfo, IDatabaseTableColumn>();

        private readonly Lazy<IReadOnlyList<IDatabaseTableColumn>> _columns;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> _columnLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>> _uniqueKeyLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> _checkLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> _indexLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> _parentKeyLookup;
        // TODO: implement triggers
        //private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggerLookup;
        private readonly Lazy<IEnumerable<IDatabaseRelationalKey>> _childKeys;
        private readonly Lazy<IDatabaseKey> _primaryKey;
    }
}
