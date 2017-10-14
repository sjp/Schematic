using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    public class ModelledRelationalDatabaseTable : IRelationalDatabaseTable, IRelationalDatabaseTableAsync
    {
        public ModelledRelationalDatabaseTable(
            IRelationalDatabase database,
            Identifier tableName,
            IReadOnlyList<IDatabaseTableColumn> columns,
            IDatabaseKey primaryKey,
            IEnumerable<IDatabaseKey> uniqueKeys,
            IEnumerable<IDatabaseRelationalKey> parentKeys,
            IEnumerable<IDatabaseRelationalKey> childKeys,
            IEnumerable<IDatabaseTableIndex> indexes,
            IEnumerable<IDatabaseCheckConstraint> checkConstraints,
            IEnumerable<IDatabaseTrigger> triggers,
            IEqualityComparer<Identifier> comparer = null)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (uniqueKeys == null || uniqueKeys.AnyNull())
                throw new ArgumentNullException(nameof(uniqueKeys));
            if (parentKeys == null || parentKeys.AnyNull())
                throw new ArgumentNullException(nameof(parentKeys));
            if (childKeys == null || childKeys.AnyNull())
                throw new ArgumentNullException(nameof(childKeys));
            if (indexes == null || indexes.AnyNull())
                throw new ArgumentNullException(nameof(indexes));
            if (checkConstraints == null || checkConstraints.AnyNull())
                throw new ArgumentNullException(nameof(checkConstraints));
            if (triggers == null || triggers.AnyNull())
                throw new ArgumentNullException(nameof(triggers));

            Database = database ?? throw new ArgumentNullException(nameof(database));

            var serverName = tableName.Server ?? database.ServerName;
            var databaseName = tableName.Database ?? database.DatabaseName;
            var schemaName = tableName.Schema ?? database.DefaultSchema;

            Name = new Identifier(serverName, databaseName, schemaName, tableName.LocalName);
            Columns = columns;
            PrimaryKey = primaryKey;
            UniqueKeys = uniqueKeys;
            ParentKeys = parentKeys;
            ChildKeys = childKeys;
            Indexes = indexes;
            CheckConstraints = checkConstraints;
            Triggers = triggers;
            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, serverName, databaseName, schemaName);

            Column = CreateColumnLookup(Columns, Comparer);
            UniqueKey = CreateUniqueKeyLookup(UniqueKeys, Comparer);
            ParentKey = CreateParentKeyLookup(ParentKeys, Comparer);
            Index = CreateIndexLookup(Indexes, Comparer);
            CheckConstraint = CreateCheckConstraintLookup(CheckConstraints, Comparer);
            Trigger = CreateTriggerLookup(Triggers, Comparer);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public IDatabaseKey PrimaryKey { get; }

        public Task<IDatabaseKey> PrimaryKeyAsync() => Task.FromResult(PrimaryKey);

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync() => Task.FromResult(Index);

        public IEnumerable<IDatabaseTableIndex> Indexes { get; }

        public Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync() => Task.FromResult(Indexes);

        private static IReadOnlyDictionary<Identifier, IDatabaseTableIndex> CreateIndexLookup(IEnumerable<IDatabaseTableIndex> indexes, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseTableIndex>(comparer);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync() => Task.FromResult(UniqueKey);

        public IEnumerable<IDatabaseKey> UniqueKeys { get; }

        public Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync() => Task.FromResult(UniqueKeys);

        private static IReadOnlyDictionary<Identifier, IDatabaseKey> CreateUniqueKeyLookup(IEnumerable<IDatabaseKey> uniqueKeys, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(comparer);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result.AsReadOnlyDictionary();
        }

        public IEnumerable<IDatabaseRelationalKey> ChildKeys { get; }

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => Task.FromResult(ChildKeys);

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckConstraintAsync() => Task.FromResult(CheckConstraint);

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints { get; }

        public Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync() => Task.FromResult(CheckConstraints);

        private static IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CreateCheckConstraintLookup(IEnumerable<IDatabaseCheckConstraint> checkConstraints, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(comparer);

            foreach (var check in checkConstraints)
                result[check.Name.LocalName] = check;

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync() => Task.FromResult(ParentKey);

        public IEnumerable<IDatabaseRelationalKey> ParentKeys { get; }

        public Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync() => Task.FromResult(ParentKeys);

        private static IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> CreateParentKeyLookup(IEnumerable<IDatabaseRelationalKey> parentKeys, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(comparer);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyList<IDatabaseTableColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync() => Task.FromResult(Columns);

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync() => Task.FromResult(Column);

        private static IReadOnlyDictionary<Identifier, IDatabaseTableColumn> CreateColumnLookup(IReadOnlyList<IDatabaseTableColumn> columns, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseTableColumn>(comparer);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IEnumerable<IDatabaseTrigger> Triggers { get; }

        public Task<IEnumerable<IDatabaseTrigger>> TriggersAsync() => Task.FromResult(Triggers);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync() => Task.FromResult(Trigger);

        private static IReadOnlyDictionary<Identifier, IDatabaseTrigger> CreateTriggerLookup(IEnumerable<IDatabaseTrigger> triggers, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(comparer);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result.AsReadOnlyDictionary();
        }
    }
}
