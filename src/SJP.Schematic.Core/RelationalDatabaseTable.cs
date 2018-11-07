using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class RelationalDatabaseTable : IRelationalDatabaseTable
    {
        public RelationalDatabaseTable(
            IRelationalDatabase database,
            Identifier tableName,
            IReadOnlyList<IDatabaseColumn> columns,
            IDatabaseKey primaryKey,
            IReadOnlyCollection<IDatabaseKey> uniqueKeys,
            IReadOnlyCollection<IDatabaseRelationalKey> parentKeys,
            IReadOnlyCollection<IDatabaseRelationalKey> childKeys,
            IReadOnlyCollection<IDatabaseIndex> indexes,
            IReadOnlyCollection<IDatabaseCheckConstraint> checks,
            IReadOnlyCollection<IDatabaseTrigger> triggers)
        {
            if (tableName == null)
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
            if (checks == null || checks.AnyNull())
                throw new ArgumentNullException(nameof(checks));
            if (triggers == null || triggers.AnyNull())
                throw new ArgumentNullException(nameof(triggers));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Columns = columns;
            PrimaryKey = primaryKey;
            UniqueKeys = uniqueKeys;
            ParentKeys = parentKeys;
            ChildKeys = childKeys;
            Indexes = indexes;
            Checks = checks;
            Triggers = triggers;

            Column = CreateColumnLookup(Columns);
            UniqueKey = CreateUniqueKeyLookup(UniqueKeys);
            ParentKey = CreateParentKeyLookup(ParentKeys);
            Index = CreateIndexLookup(Indexes);
            Check = CreateCheckLookup(Checks);
            Trigger = CreateTriggerLookup(Triggers);
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        public IDatabaseKey PrimaryKey { get; }

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(PrimaryKey);

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Index);

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Indexes);

        private static IReadOnlyDictionary<Identifier, IDatabaseIndex> CreateIndexLookup(IReadOnlyCollection<IDatabaseIndex> indexes)
        {
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(UniqueKey);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(UniqueKeys);

        private static IReadOnlyDictionary<Identifier, IDatabaseKey> CreateUniqueKeyLookup(IReadOnlyCollection<IDatabaseKey> uniqueKeys)
        {
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result;
        }

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(ChildKeys);

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Check);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Checks);

        private static IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CreateCheckLookup(IReadOnlyCollection<IDatabaseCheckConstraint> checks)
        {
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(ParentKey);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(ParentKeys);

        private static IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> CreateParentKeyLookup(IReadOnlyCollection<IDatabaseRelationalKey> parentKeys)
        {
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result;
        }

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Columns);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Column);

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> CreateColumnLookup(IReadOnlyList<IDatabaseColumn> columns)
        {
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Triggers);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Trigger);

        private static IReadOnlyDictionary<Identifier, IDatabaseTrigger> CreateTriggerLookup(IReadOnlyCollection<IDatabaseTrigger> triggers)
        {
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result;
        }
    }
}
