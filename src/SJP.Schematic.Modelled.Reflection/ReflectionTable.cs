using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTable : IRelationalDatabaseTable
    {
        public ReflectionTable(IRelationalDatabase database, Type tableType)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            InstanceType = tableType ?? throw new ArgumentNullException(nameof(tableType));
            Dialect = Database.Dialect;
            Name = Dialect.GetQualifiedNameOrDefault(database, InstanceType);
            TypeProvider = new ReflectionTableTypeProvider(Dialect, InstanceType);

            _columns = new Lazy<IReadOnlyList<IDatabaseTableColumn>>(LoadColumnList);
            _columnLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>>(LoadColumns);
            _checkLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>>(LoadChecks);
            _uniqueKeyLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>>(LoadUniqueKeys);
            _indexLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>>(LoadIndexes);
            _parentKeyLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>>(LoadParentKeys);
            _childKeys = new Lazy<IReadOnlyCollection<IDatabaseRelationalKey>>(LoadChildKeys);
            _primaryKey = new Lazy<IDatabaseKey>(LoadPrimaryKey);
        }

        protected IDatabaseDialect Dialect { get; }

        protected ReflectionTableTypeProvider TypeProvider { get; }

        private IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeys()
        {
            return Database.Tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ParentKey.Table.Name == Name)
                .ToList();
        }

        // TODO: see if it's possible to create a foreign key to a synonym in sql server
        //       this should be possible in oracle but not sure
        private IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeys()
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>();

            var parentKeys = TypeProvider.ParentKeys;
            if (parentKeys.Empty())
                return result;

            foreach (var declaredParentKey in parentKeys)
            {
                var fkColumns = declaredParentKey.Columns.Select(GetColumn).ToList();

                var parentName = Dialect.GetQualifiedNameOrDefault(Database, declaredParentKey.TargetType);
                var parent = Database.GetTable(parentName);

                var parentTypeProvider = new ReflectionTableTypeProvider(Dialect, declaredParentKey.TargetType);
                var parentInstance = parentTypeProvider.TableInstance;
                var keyObject = declaredParentKey.KeySelector(parentInstance);
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
                var childKeyName = Dialect.GetAliasOrDefault(declaredParentKey.Property);
                var childKey = new ReflectionForeignKey(this, childKeyName, parentKey, fkColumns);

                var deleteAttr = Dialect.GetDialectAttribute<OnDeleteRuleAttribute>(declaredParentKey.Property);
                var deleteRule = deleteAttr?.Rule ?? Rule.None;

                var updateAttr = Dialect.GetDialectAttribute<OnUpdateRuleAttribute>(declaredParentKey.Property);
                var updateRule = updateAttr?.Rule ?? Rule.None;

                result[childKey.Name.LocalName] = new ReflectionRelationalKey(childKey, parentKey, deleteRule, updateRule);
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyList<IDatabaseTableColumn> LoadColumnList()
        {
            return TypeProvider.Columns
                .Select(GetColumn)
                .ToList()
                .AsReadOnly();
        }

        private IDatabaseTableColumn GetColumn(IModelledColumn column)
        {
            if (column.IsComputed && column is IModelledComputedColumn computedColumn)
            {
                var computedName = Dialect.GetAliasOrDefault(computedColumn.Property);
                var definition = computedColumn.Expression.ToSql(Dialect);
                return new ReflectionTableComputedColumn(Dialect, this, computedName, definition);
            }
            else
            {
                return new ReflectionTableColumn(Dialect, this, column.Property, column.DeclaredDbType, column.IsNullable);
            }
        }

        protected Type InstanceType { get; }

        private IDatabaseKey LoadPrimaryKey()
        {
            var primaryKey = TypeProvider.PrimaryKey;
            var dialect = Database.Dialect;
            var pkColumns = primaryKey.Columns.Select(GetColumn).ToList();

            var keyName = dialect.GetAliasOrDefault(primaryKey.Property);
            return new ReflectionKey(this, keyName, primaryKey.KeyType, pkColumns);
        }

        private IReadOnlyDictionary<Identifier, IDatabaseTableIndex> LoadIndexes()
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>();

            foreach (var index in TypeProvider.Indexes)
            {
                var refIndex = new ReflectionDatabaseTableIndex(this, index);
                result[refIndex.Name.LocalName] = refIndex;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeys()
        {
            var result = new Dictionary<Identifier, IDatabaseKey>();

            var uniqueKeys = TypeProvider.UniqueKeys;
            if (uniqueKeys.Empty())
                return result;

            foreach (var uniqueKey in uniqueKeys)
            {
                var ukColumns = uniqueKey.Columns.Select(GetColumn).ToList();
                var keyName = Dialect.GetAliasOrDefault(uniqueKey.Property);
                var uk = new ReflectionKey(this, keyName, uniqueKey.KeyType, ukColumns);
                result[uk.Name.LocalName] = uk;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadChecks()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>();

            var dialect = Database.Dialect;
            foreach (var modelledCheck in TypeProvider.Checks)
            {
                var definition = modelledCheck.Expression.ToSql(dialect);
                var checkName = dialect.GetAliasOrDefault(modelledCheck.Property);
                var check = new ReflectionCheckConstraint(this, checkName, definition);
                result[check.Name.LocalName] = check;
            }

            return result.AsReadOnlyDictionary();
        }

        private IReadOnlyDictionary<Identifier, IDatabaseTableColumn> LoadColumns()
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>();

            foreach (var column in TypeProvider.Columns)
            {
                var col = GetColumn(column);
                result[col.Name.LocalName] = col;
            }

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => _checkLookup.Value;

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => new ReadOnlyCollectionSlim<IDatabaseCheckConstraint>(_checkLookup.Value.Count, _checkLookup.Value.Values);

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => _childKeys.Value;

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column => _columnLookup.Value;

        public IReadOnlyList<IDatabaseTableColumn> Columns => _columns.Value;

        public IRelationalDatabase Database { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => _indexLookup.Value;

        public IReadOnlyCollection<IDatabaseTableIndex> Indexes => new ReadOnlyCollectionSlim<IDatabaseTableIndex>(_indexLookup.Value.Count, _indexLookup.Value.Values);

        public Identifier Name { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => _parentKeyLookup.Value;

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => new ReadOnlyCollectionSlim<IDatabaseRelationalKey>(_parentKeyLookup.Value.Count, _parentKeyLookup.Value.Values);

        public IDatabaseKey PrimaryKey => _primaryKey.Value;

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => _uniqueKeyLookup.Value;

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => new ReadOnlyCollectionSlim<IDatabaseKey>(_uniqueKeyLookup.Value.Count, _uniqueKeyLookup.Value.Values);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_checkLookup.Value);

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(
            new ReadOnlyCollectionSlim<IDatabaseCheckConstraint>(_checkLookup.Value.Count, _checkLookup.Value.Values)
        );

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_childKeys.Value);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_columnLookup.Value);

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = _columnLookup.Value.Values.ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<IDatabaseTableColumn>>(result);
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_indexLookup.Value);

        public Task<IReadOnlyCollection<IDatabaseTableIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseTableIndex>>(
            new ReadOnlyCollectionSlim<IDatabaseTableIndex>(_indexLookup.Value.Count, _indexLookup.Value.Values)
        );

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_parentKeyLookup.Value);

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseRelationalKey>>(
            new ReadOnlyCollectionSlim<IDatabaseRelationalKey>(_parentKeyLookup.Value.Count, _parentKeyLookup.Value.Values)
        );

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_primaryKey.Value);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_uniqueKeyLookup.Value);

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseKey>>(
            new ReadOnlyCollectionSlim<IDatabaseKey>(_uniqueKeyLookup.Value.Count, _uniqueKeyLookup.Value.Values)
        );

        private readonly Lazy<IReadOnlyList<IDatabaseTableColumn>> _columns;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> _columnLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>> _uniqueKeyLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> _checkLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> _indexLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> _parentKeyLookup;
        // TODO: implement triggers
        //private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggerLookup;
        private readonly Lazy<IReadOnlyCollection<IDatabaseRelationalKey>> _childKeys;
        private readonly Lazy<IDatabaseKey> _primaryKey;
    }
}
