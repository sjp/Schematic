using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            _columns = new Lazy<IReadOnlyList<IDatabaseColumn>>(LoadColumnList);
            _columnLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseColumn>>(LoadColumns);
            _checkLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>>(LoadChecks);
            _uniqueKeyLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>>(LoadUniqueKeys);
            _indexLookup = new Lazy<IReadOnlyDictionary<Identifier, IDatabaseIndex>>(LoadIndexes);
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
                .Where(fk => fk.ParentTable == Name)
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
                var parentOption = Database.GetTable(parentName);
                if (parentOption.IsNone)
                    throw new Exception("Could not find parent table with name: " + parentName.ToString());

                var parent = parentOption.UnwrapSome();

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
                    var uniqueKey = parent.UniqueKeys.FirstOrDefault(uk => uk.Name == parentKeyName);
                    if (uniqueKey == null)
                        throw new Exception("Could not find matching parent key for foreign key."); // same goes for this...

                    parentKey = uniqueKey;
                }
                else
                {
                    throw new Exception("Cannot point a foreign key to a key that is not the primary key or a unique key."); // TODO: improve error messaging
                }

                // check that columns match up with parent key -- otherwise will fail
                // TODO: don't assume that the FK is to a table -- could be to a synonym
                //       maybe change interface of Synonym<T> to be something like Synonym<Table<T>> or Synonym<Synonym<T>> -- could unwrap at runtime?
                var childKeyName = Dialect.GetAliasOrDefault(declaredParentKey.Property);
                var childKey = new ReflectionForeignKey(childKeyName, parentKey, fkColumns);

                var deleteAttr = Dialect.GetDialectAttribute<OnDeleteRuleAttribute>(declaredParentKey.Property);
                var deleteRule = deleteAttr?.Rule ?? Rule.None;

                var updateAttr = Dialect.GetDialectAttribute<OnUpdateRuleAttribute>(declaredParentKey.Property);
                var updateRule = updateAttr?.Rule ?? Rule.None;

                result[childKey.Name.LocalName] = new ReflectionRelationalKey(Name, childKey, parent.Name, parentKey, deleteRule, updateRule);
            }

            return result;
        }

        private IReadOnlyList<IDatabaseColumn> LoadColumnList()
        {
            return TypeProvider.Columns
                .Select(GetColumn)
                .ToList()
                .AsReadOnly();
        }

        private IDatabaseColumn GetColumn(IModelledColumn column)
        {
            if (column.IsComputed && column is IModelledComputedColumn computedColumn)
            {
                var computedName = Dialect.GetAliasOrDefault(computedColumn.Property);
                var definition = computedColumn.Expression.ToSql(Dialect);
                return new ReflectionTableComputedColumn(Dialect, this, computedName, definition);
            }
            else
            {
                return new ReflectionTableColumn(Dialect, column.Property, column.DeclaredDbType, column.IsNullable);
            }
        }

        protected Type InstanceType { get; }

        private IDatabaseKey LoadPrimaryKey()
        {
            var primaryKey = TypeProvider.PrimaryKey;
            var dialect = Database.Dialect;
            var pkColumns = primaryKey.Columns.Select(GetColumn).ToList();

            var keyName = dialect.GetAliasOrDefault(primaryKey.Property);
            return new ReflectionKey(keyName, primaryKey.KeyType, pkColumns);
        }

        private IReadOnlyDictionary<Identifier, IDatabaseIndex> LoadIndexes()
        {
            var result = new Dictionary<Identifier, IDatabaseIndex>();

            foreach (var index in TypeProvider.Indexes)
            {
                var refIndex = new ReflectionDatabaseTableIndex(Database.Dialect, this, index);
                result[refIndex.Name.LocalName] = refIndex;
            }

            return result;
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
                var uk = new ReflectionKey(keyName, uniqueKey.KeyType, ukColumns);
                result[uk.Name.LocalName] = uk;
            }

            return result;
        }

        private IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadChecks()
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>();

            var dialect = Database.Dialect;
            foreach (var modelledCheck in TypeProvider.Checks)
            {
                var definition = modelledCheck.Expression.ToSql(dialect);
                var checkName = dialect.GetAliasOrDefault(modelledCheck.Property);
                var check = new ReflectionCheckConstraint(checkName, definition);
                result[check.Name.LocalName] = check;
            }

            return result;
        }

        private IReadOnlyDictionary<Identifier, IDatabaseColumn> LoadColumns()
        {
            var result = new Dictionary<Identifier, IDatabaseColumn>();

            foreach (var column in TypeProvider.Columns)
            {
                var col = GetColumn(column);
                result[col.Name.LocalName] = col;
            }

            return result;
        }

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => new ReadOnlyCollectionSlim<IDatabaseCheckConstraint>(_checkLookup.Value.Count, _checkLookup.Value.Values);

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => _childKeys.Value;

        public IReadOnlyList<IDatabaseColumn> Columns => _columns.Value;

        protected IRelationalDatabase Database { get; }

        public IReadOnlyCollection<IDatabaseIndex> Indexes => new ReadOnlyCollectionSlim<IDatabaseIndex>(_indexLookup.Value.Count, _indexLookup.Value.Values);

        public Identifier Name { get; }

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => new ReadOnlyCollectionSlim<IDatabaseRelationalKey>(_parentKeyLookup.Value.Count, _parentKeyLookup.Value.Values);

        public IDatabaseKey PrimaryKey => _primaryKey.Value;

        public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => new ReadOnlyCollectionSlim<IDatabaseKey>(_uniqueKeyLookup.Value.Count, _uniqueKeyLookup.Value.Values);

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(
            new ReadOnlyCollectionSlim<IDatabaseCheckConstraint>(_checkLookup.Value.Count, _checkLookup.Value.Values)
        );

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_childKeys.Value);

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = _columnLookup.Value.Values.ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<IDatabaseColumn>>(result);
        }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseIndex>>(
            new ReadOnlyCollectionSlim<IDatabaseIndex>(_indexLookup.Value.Count, _indexLookup.Value.Values)
        );

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseRelationalKey>>(
            new ReadOnlyCollectionSlim<IDatabaseRelationalKey>(_parentKeyLookup.Value.Count, _parentKeyLookup.Value.Values)
        );

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(_primaryKey.Value);

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<IDatabaseKey>>(
            new ReadOnlyCollectionSlim<IDatabaseKey>(_uniqueKeyLookup.Value.Count, _uniqueKeyLookup.Value.Values)
        );

        private readonly Lazy<IReadOnlyList<IDatabaseColumn>> _columns;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseColumn>> _columnLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseKey>> _uniqueKeyLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> _checkLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseIndex>> _indexLookup;
        private readonly Lazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> _parentKeyLookup;
        // TODO: implement triggers
        //private readonly Lazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggerLookup;
        private readonly Lazy<IReadOnlyCollection<IDatabaseRelationalKey>> _childKeys;
        private readonly Lazy<IDatabaseKey> _primaryKey;
    }
}
