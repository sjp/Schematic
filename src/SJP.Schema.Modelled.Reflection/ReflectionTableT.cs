using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTable<T> : IRelationalDatabaseTable where T : class, new()
    {
        public ReflectionTable(IRelationalDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));

            _dependencies = new Lazy<IEnumerable<Identifier>>(LoadDependencies);
            _dependents = new Lazy<IEnumerable<Identifier>>(LoadDependents);

            _columns = new Lazy<IList<IDatabaseTableColumn>>(LoadColumnList);
            _columnLookup = new Lazy<IReadOnlyDictionary<string, IDatabaseTableColumn>>(LoadColumns);
            _checkLookup = new Lazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>>(LoadChecks);
            _uniqueKeyLookup = new Lazy<IReadOnlyDictionary<string, IDatabaseKey>>(LoadUniqueKeys);
            _indexLookup = new Lazy<IReadOnlyDictionary<string, IDatabaseTableIndex>>(LoadIndexes);
            _parentKeyLookup = new Lazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>>(LoadParentKeys);
            _childKeys = new Lazy<IEnumerable<IDatabaseRelationalKey>>(LoadChildKeys);
            _primaryKey = new Lazy<IDatabaseKey>(LoadPrimaryKey);

            var db = database as IReflectionRelationalDatabase;
            TableInstance = db.TableInstances[InstanceType] as T;

            Name = Database.Dialect.GetQualifiedNameOverrideOrDefault(database, InstanceType);
        }

        // TODO: handle situation where a view depends on this table
        //       maybe this would require a connection to the database?
        private IEnumerable<Identifier> LoadDependents()
        {
            var childTables = ChildKeys
                .Select(fk => fk.ChildKey.Table.Name)
                .Where(name => name != Name)
                .Distinct()
                .ToList();

            return childTables.ToImmutableList();
        }

        // TODO: handle situation where the table has a foreign key to a view? would this happen in sql server??
        //       maybe this would require a connection to the database?
        private IEnumerable<Identifier> LoadDependencies()
        {
            var parentTables = ParentKeys
                .Select(fk => fk.ParentKey.Table.Name)
                .Where(name => name != Name)
                .Distinct()
                .ToList();

            return parentTables.ToImmutableList();
        }

        private IEnumerable<IDatabaseRelationalKey> LoadChildKeys()
        {
            var result = new List<IDatabaseRelationalKey>();

            // TODO! this needs to be changed so we get dependent types injected somehow...
            var reflectionDb = Database as IReflectionRelationalDatabase;
            if (reflectionDb == null)
                throw new Exception("Child key loading will not work without the database being a reflection database...");

            foreach (var tableInstance in reflectionDb.TableInstances)
            {
                var keyProps = tableInstance.Key.GetTypeInfo().DeclaredProperties.Where(IsKeyProperty);
                foreach (var keyProp in keyProps)
                {
                    // TODO need to get the names resolved properly to handle overrides
                    var keyInstance = keyProp.GetValue(tableInstance.Value) as IModelledKey;
                    if (keyInstance.KeyType != DatabaseKeyType.Foreign)
                        continue;

                    var fkeyInstance = keyInstance as Key.ForeignKey;
                    fkeyInstance.Property = keyProp;
                    if (fkeyInstance.TargetType != InstanceType)
                        continue;

                    var childTable = Database.Table[tableInstance.Key.Name];
                    var childKey = childTable.ParentKey[keyProp.Name];
                    result.Add(childKey);
                }
            }

            return result.ToImmutableList();
        }

        // TODO: see if it's possible to create a foreign key to a synonym in sql server
        //       this should be possible in oracle but not sure
        private IReadOnlyDictionary<string, IDatabaseRelationalKey> LoadParentKeys()
        {
            var result = new Dictionary<string, IDatabaseRelationalKey>();
            var tableColumns = Column; // trigger load

            var keyProperties = InstanceType
                .GetTypeInfo()
                .DeclaredProperties
                .Where(IsKeyProperty);

            var fks = new List<Key.ForeignKey>();
            foreach (var keyProperty in keyProperties)
            {
                var keyValue = keyProperty.GetValue(TableInstance) as Key.ForeignKey;
                if (keyValue == null)
                    continue;
                //throw new InvalidCastException($"Expected to find a key type that implements { typeof(IModelledKey).FullName } on { InstanceType.FullName }.{ keyProperty.Name }.");

                keyValue.Property = keyProperty;
                if (keyValue.KeyType == DatabaseKeyType.Foreign)
                    fks.Add(keyValue);
            }

            if (fks.Count == 0)
                return result;

            var dialect = Database.Dialect;
            var reflectionDb = Database as IReflectionRelationalDatabase;

            foreach (var fk in fks)
            {
                var fkColumns = fk.Columns
                    .Select(c => c.Property)
                    .Where(PropertyColumnCache.ContainsKey)
                    .Select(prop => PropertyColumnCache[prop]);

                // fixing naming so that we can override, incl by dialect
                var parentName = fk.TargetType.Name;
                var parent = Database.Table[parentName];

                var parentInstance = reflectionDb.TableInstances[fk.TargetType];
                var keyObject = fk.KeySelector(parentInstance);
                var parentKeyName = dialect.GetNameOverrideOrDefault(keyObject.Property);

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
                    throw new Exception("Cannot point a foreign key to a key that is not the primary key or a unique key.");
                }

                // check that columns match up with parent key -- otherwise will fail
                // TODO: don't assume that the FK is to a table -- could be to a synonym
                //       maybe change interface of Synonym<T> to be something like Synonym<Table<T>> or Synonym<Synonym<T>> -- could unwrap at runtime?
                var childKey = new ReflectionForeignKey(this, parentKey, fk.Property, fkColumns);

                var deleteAttr = dialect.GetDialectAttribute<OnDeleteActionAttribute>(fk.Property);
                var deleteAction = deleteAttr != null ? _updateActionMapping[deleteAttr.Action] : RelationalKeyUpdateAction.NoAction;

                var updateAttr = dialect.GetDialectAttribute<OnDeleteActionAttribute>(fk.Property);
                var updateAction = updateAttr != null ? _updateActionMapping[updateAttr.Action] : RelationalKeyUpdateAction.NoAction;

                // TODO: check that column lists are type compatible...
                result[childKey.Name.LocalName] = new ReflectionRelationalKey(childKey, parentKey, deleteAction, updateAction);
            }

            return result.ToImmutableDictionary();
        }

        private readonly static IReadOnlyDictionary<ForeignKeyAction, RelationalKeyUpdateAction> _updateActionMapping = new Dictionary<ForeignKeyAction, RelationalKeyUpdateAction>
        {
            [ForeignKeyAction.NoAction] = RelationalKeyUpdateAction.NoAction,
            [ForeignKeyAction.Cascade] = RelationalKeyUpdateAction.Cascade,
            [ForeignKeyAction.SetNull] = RelationalKeyUpdateAction.SetNull,
            [ForeignKeyAction.SetDefault] = RelationalKeyUpdateAction.SetDefault
        };

        public T TableInstance { get; }

        private IList<IDatabaseTableColumn> LoadColumnList()
        {
            return InstanceProperties
                .Where(PropertyColumnCache.ContainsKey)
                .Select(pi => PropertyColumnCache[pi])
                .ToImmutableList();
        }

        protected static Type InstanceType { get; } = typeof(T);

        protected static ISet<PropertyInfo> InstanceProperties { get; } = InstanceType.GetTypeInfo().DeclaredProperties.ToImmutableHashSet();

        private IDatabaseKey LoadPrimaryKey()
        {
            var tableColumns = Column; // trigger column load

            var keyProperties = InstanceProperties.Where(IsKeyProperty);

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
                .Select(c => dialect.GetNameOverrideOrDefault(c.Property))
                .Where(name => Column.ContainsKey(name))
                .Select(name => Column[name]);

            return new ReflectionKey(this, primaryKey.Property, pkColumns, primaryKey.KeyType);
        }

        private IReadOnlyDictionary<string, IDatabaseTableIndex> LoadIndexes()
        {
            var result = new Dictionary<string, IDatabaseTableIndex>();

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

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<string, IDatabaseKey> LoadUniqueKeys()
        {
            var result = new Dictionary<string, IDatabaseKey>();
            var tableColumns = Column; // trigger load

            var keyProperties = InstanceProperties.Where(IsKeyProperty);

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
                    .Where(PropertyColumnCache.ContainsKey)
                    .Select(prop => PropertyColumnCache[prop]);

                var uk = new ReflectionKey(this, uniqueKey.Property, ukColumns, uniqueKey.KeyType);
                result[uk.Name.LocalName] = uk;
            }

            // TODO add check to ensure that column lists do not match primary key

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<string, IDatabaseCheckConstraint> LoadChecks()
        {
            var result = new Dictionary<string, IDatabaseCheckConstraint>();

            var dialect = Database.Dialect;
            var checkProperties = InstanceProperties.Where(IsCheckProperty);

            foreach (var checkProperty in checkProperties)
            {
                var modelledCheck = checkProperty.GetValue(TableInstance) as IModelledCheckConstraint;
                modelledCheck.Property = checkProperty;

                var columns = modelledCheck.Expression.DependentNames
                    .Where(name => Column.ContainsKey(name.LocalName))
                    .Select(name => Column[name.LocalName]);

                // TODO: check name
                var checkName = dialect.GetNameOverrideOrDefault(checkProperty);
                var check = new ReflectionCheckConstraint(this, checkName, modelledCheck.Expression, columns);
                result[check.Name.LocalName] = check;
            }

            return result.ToImmutableDictionary();
        }

        private IReadOnlyDictionary<string, IDatabaseTableColumn> LoadColumns()
        {
            var result = new Dictionary<string, IDatabaseTableColumn>();

            var columnTypes = InstanceProperties
                .Where(IsColumnProperty)
                .Select(pi => pi.GetValue(TableInstance) as IModelledColumn);

            foreach (var modelledColumn in columnTypes)
            {
                var column = new ReflectionTableColumn(this, modelledColumn.Property, modelledColumn.DbType, modelledColumn.IsNullable);
                result[column.Name.LocalName] = column;
                PropertyColumnCache[modelledColumn.Property] = column;
            }

            return result.ToImmutableDictionary();
        }

        private static bool IsColumnProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsGenericType
            && prop.PropertyType.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(ColumnType.GetTypeInfo());

        private static bool IsIndexProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(IndexType.GetTypeInfo());

        private static bool IsKeyProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(KeyType.GetTypeInfo());

        private static bool IsCheckProperty(PropertyInfo prop) =>
            prop.PropertyType.GetTypeInfo().IsAssignableFrom(CheckType.GetTypeInfo());

        private static Type ColumnType { get; } = typeof(Column<>);

        private static Type KeyType { get; } = typeof(Key);

        private static Type CheckType { get; } = typeof(Check);

        private static Type IndexType { get; } = typeof(Index);

        private static Type DbTypeArg { get; } = typeof(IDbType);

        public IReadOnlyDictionary<string, IDatabaseCheckConstraint> CheckConstraint => _checkLookup.Value;

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => _checkLookup.Value.Values;

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => _childKeys.Value;

        public IReadOnlyDictionary<string, IDatabaseTableColumn> Column => _columnLookup.Value;

        public IList<IDatabaseTableColumn> Columns => _columns.Value;

        public IRelationalDatabase Database { get; }

        public IReadOnlyDictionary<string, IDatabaseTableIndex> Index => _indexLookup.Value;

        public IEnumerable<IDatabaseTableIndex> Indexes => _indexLookup.Value.Values;

        public Identifier Name { get; }

        public IReadOnlyDictionary<string, IDatabaseRelationalKey> ParentKey => _parentKeyLookup.Value;

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => _parentKeyLookup.Value.Values;

        public IDatabaseKey PrimaryKey => _primaryKey.Value;

        public IReadOnlyDictionary<string, IDatabaseTrigger> Trigger { get; }

        public IEnumerable<IDatabaseTrigger> Triggers { get; }

        public IReadOnlyDictionary<string, IDatabaseKey> UniqueKey => _uniqueKeyLookup.Value;

        public IEnumerable<IDatabaseKey> UniqueKeys => _uniqueKeyLookup.Value.Values;

        public Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> CheckConstraintAsync() => Task.FromResult(_checkLookup.Value);

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => Task.FromResult(_checkLookup.Value.Values);

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => Task.FromResult(_childKeys.Value);

        public Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> ColumnAsync() => Task.FromResult(_columnLookup.Value);

        public Task<IList<IDatabaseTableColumn>> ColumnsAsync()
        {
            IList<IDatabaseTableColumn> result = _columnLookup.Value.Values.ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> IndexAsync() => Task.FromResult(_indexLookup.Value);

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => Task.FromResult(_indexLookup.Value.Values);

        public Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> ParentKeyAsync() => Task.FromResult(_parentKeyLookup.Value);

        public Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync() => Task.FromResult(_parentKeyLookup.Value.Values);

        public Task<IDatabaseKey> PrimaryKeyAsync() => Task.FromResult(_primaryKey.Value);

        public Task<IReadOnlyDictionary<string, IDatabaseTrigger>> TriggerAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDatabaseTrigger>> TriggersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, IDatabaseKey>> UniqueKeyAsync() => Task.FromResult(_uniqueKeyLookup.Value);

        public Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync() => Task.FromResult(_uniqueKeyLookup.Value.Values);

        public Task<IEnumerable<Identifier>> DependenciesAsync() => Task.FromResult(_dependencies.Value);

        public Task<IEnumerable<Identifier>> DependentsAsync() => Task.FromResult(_dependents.Value);

        private IDictionary<PropertyInfo, IDatabaseTableColumn> PropertyColumnCache { get; } = new Dictionary<PropertyInfo, IDatabaseTableColumn>();

        public IEnumerable<Identifier> Dependencies => _dependencies.Value;

        public IEnumerable<Identifier> Dependents => _dependents.Value;

        public static void PopulateProperties(T tableInstance) => PopulateColumnProperties(tableInstance);

        private static void PopulateColumnProperties(T tableInstance)
        {
            var columnProps = InstanceProperties.Where(IsColumnProperty);

            foreach (var prop in columnProps)
            {
                var field = prop.GetAutoBackingField();
                if (field == null)
                    throw new ArgumentException($"The column property { InstanceType.FullName }.{ prop.Name } must be an auto-implemented property. For example: Column<T> { prop.Name } {{ get; }}");

                var columnDbType = field.FieldType;
                var columnPropertyProp = columnDbType.GetTypeInfo().GetProperty(nameof(IModelledColumn.Property), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var columnInstance = Activator.CreateInstance(columnDbType);
                columnPropertyProp.SetValue(columnInstance, prop);
                field.SetValue(tableInstance, columnInstance);
            }
        }

        private readonly Lazy<IEnumerable<Identifier>> _dependencies;
        private readonly Lazy<IEnumerable<Identifier>> _dependents;

        private readonly Lazy<IList<IDatabaseTableColumn>> _columns;
        private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTableColumn>> _columnLookup;
        // TODO: implement triggers
        //private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggerLookup;
        private readonly Lazy<IReadOnlyDictionary<string, IDatabaseKey>> _uniqueKeyLookup;
        private readonly Lazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> _checkLookup;
        private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTableIndex>> _indexLookup;
        private readonly Lazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>> _parentKeyLookup;
        private readonly Lazy<IEnumerable<IDatabaseRelationalKey>> _childKeys;
        private readonly Lazy<IDatabaseKey> _primaryKey;
    }
}
