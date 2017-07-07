using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.Core
{
    public class CachedRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public CachedRelationalDatabaseTable(IRelationalDatabase database, IRelationalDatabaseTable table, IdentifierComparer comparer)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Table = table ?? throw new ArgumentNullException(nameof(table));
            comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

            Name = Table.Name;

            _primaryKey = new AsyncLazy<IDatabaseKey>(Table.PrimaryKeyAsync);
            _columnLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableColumn>>(Table.ColumnAsync);
            _columns = new AsyncLazy<IList<IDatabaseTableColumn>>(Table.ColumnsAsync);
            _uniqueKeys = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseKey>>(Table.UniqueKeyAsync);
            _indexes = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableIndex>>(Table.IndexAsync);
            _parentKeys = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>>(Table.ParentKeyAsync);
            _childKeys = new AsyncLazy<IEnumerable<IDatabaseRelationalKey>>(Table.ChildKeysAsync);
            _checks = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>>(Table.CheckConstraintAsync);
            _triggers = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseTrigger>>(Table.TriggerAsync);
        }

        public IRelationalDatabase Database { get; }

        protected IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public IDatabaseKey PrimaryKey => _primaryKey.Task.Result;

        public IReadOnlyDictionary<string, IDatabaseTableColumn> Column => _columnLookup.Task.Result;

        public IList<IDatabaseTableColumn> Columns => _columns.Task.Result;

        public IReadOnlyDictionary<string, IDatabaseCheckConstraint> CheckConstraint => _checks.Task.Result;

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => _checks.Task.Result.Values;

        public IReadOnlyDictionary<string, IDatabaseTableIndex> Index => _indexes.Task.Result;

        public IEnumerable<IDatabaseTableIndex> Indexes => _indexes.Task.Result.Values;

        public IReadOnlyDictionary<string, IDatabaseKey> UniqueKey => _uniqueKeys.Task.Result;

        public IEnumerable<IDatabaseKey> UniqueKeys => _uniqueKeys.Task.Result.Values;

        public IReadOnlyDictionary<string, IDatabaseRelationalKey> ParentKey => _parentKeys.Task.Result;

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => _parentKeys.Task.Result.Values;

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => _childKeys.Task.Result;

        public IReadOnlyDictionary<string, IDatabaseTrigger> Trigger => _triggers.Task.Result;

        public IEnumerable<IDatabaseTrigger> Triggers => _triggers.Task.Result.Values;

        public Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> CheckConstraintAsync() => _checks.Task;

        public async Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync()
        {
            var checks = await _checks;
            return checks.Values;
        }

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => _childKeys.Task;

        public Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> ColumnAsync() => _columnLookup.Task;

        public Task<IList<IDatabaseTableColumn>> ColumnsAsync() => _columns.Task;

        public Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> IndexAsync() => _indexes.Task;

        public async Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync()
        {
            var indexes = await _indexes;
            return indexes.Values;
        }

        public Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> ParentKeyAsync() => _parentKeys.Task;

        public async Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync()
        {
            var parentKeys = await _parentKeys;
            return parentKeys.Values;
        }

        public Task<IDatabaseKey> PrimaryKeyAsync() => _primaryKey.Task;

        public Task<IReadOnlyDictionary<string, IDatabaseTrigger>> TriggerAsync() => _triggers.Task;

        public async Task<IEnumerable<IDatabaseTrigger>> TriggersAsync()
        {
            var triggers = await _triggers;
            return triggers.Values;
        }

        public Task<IReadOnlyDictionary<string, IDatabaseKey>> UniqueKeyAsync() => _uniqueKeys.Task;

        public async Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync()
        {
            var uniqueKeys = await _uniqueKeys;
            return uniqueKeys.Values;
        }

        private readonly AsyncLazy<IDatabaseKey> _primaryKey;
        private readonly AsyncLazy<IList<IDatabaseTableColumn>> _columns;
        private readonly AsyncLazy<IEnumerable<IDatabaseRelationalKey>> _childKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableColumn>> _columnLookup;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseKey>> _uniqueKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTableIndex>> _indexes;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseRelationalKey>> _parentKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> _checks;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseTrigger>> _triggers;
    }
}
