using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.Core
{
    public class CachedRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public CachedRelationalDatabaseTable(IRelationalDatabase database, IRelationalDatabaseTable table)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = Table.Name;

            _primaryKey = new AsyncLazy<IDatabaseKey>(Table.PrimaryKeyAsync);
            _columnLookup = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>>(Table.ColumnAsync);
            _columns = new AsyncLazy<IReadOnlyList<IDatabaseTableColumn>>(Table.ColumnsAsync);
            _uniqueKeys = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseKey>>(Table.UniqueKeyAsync);
            _indexes = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>>(Table.IndexAsync);
            _parentKeys = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>>(Table.ParentKeyAsync);
            _childKeys = new AsyncLazy<IEnumerable<IDatabaseRelationalKey>>(Table.ChildKeysAsync);
            _checks = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>>(Table.CheckConstraintAsync);
            _triggers = new AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTrigger>>(Table.TriggerAsync);
        }

        public IRelationalDatabase Database { get; }

        protected IRelationalDatabaseTable Table { get; }

        public Identifier Name { get; }

        public IDatabaseKey PrimaryKey => _primaryKey.Task.Result;

        public IReadOnlyDictionary<Identifier, IDatabaseTableColumn> Column => _columnLookup.Task.Result;

        public IReadOnlyList<IDatabaseTableColumn> Columns => _columns.Task.Result;

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> CheckConstraint => _checks.Task.Result;

        public IEnumerable<IDatabaseCheckConstraint> CheckConstraints => _checks.Task.Result.Values;

        public IReadOnlyDictionary<Identifier, IDatabaseTableIndex> Index => _indexes.Task.Result;

        public IEnumerable<IDatabaseTableIndex> Indexes => _indexes.Task.Result.Values;

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => _uniqueKeys.Task.Result;

        public IEnumerable<IDatabaseKey> UniqueKeys => _uniqueKeys.Task.Result.Values;

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => _parentKeys.Task.Result;

        public IEnumerable<IDatabaseRelationalKey> ParentKeys => _parentKeys.Task.Result.Values;

        public IEnumerable<IDatabaseRelationalKey> ChildKeys => _childKeys.Task.Result;

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => _triggers.Task.Result;

        public IEnumerable<IDatabaseTrigger> Triggers => _triggers.Task.Result.Values;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckConstraintAsync() => _checks.Task;

        public async Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync()
        {
            var checks = await _checks;
            return checks.Values;
        }

        public Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync() => _childKeys.Task;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync() => _columnLookup.Task;

        public Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync() => _columns.Task;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync() => _indexes.Task;

        public async Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync()
        {
            var indexes = await _indexes;
            return indexes.Values;
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync() => _parentKeys.Task;

        public async Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync()
        {
            var parentKeys = await _parentKeys;
            return parentKeys.Values;
        }

        public Task<IDatabaseKey> PrimaryKeyAsync() => _primaryKey.Task;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync() => _triggers.Task;

        public async Task<IEnumerable<IDatabaseTrigger>> TriggersAsync()
        {
            var triggers = await _triggers;
            return triggers.Values;
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync() => _uniqueKeys.Task;

        public async Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync()
        {
            var uniqueKeys = await _uniqueKeys;
            return uniqueKeys.Values;
        }

        private readonly AsyncLazy<IDatabaseKey> _primaryKey;
        private readonly AsyncLazy<IReadOnlyList<IDatabaseTableColumn>> _columns;
        private readonly AsyncLazy<IEnumerable<IDatabaseRelationalKey>> _childKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> _columnLookup;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseKey>> _uniqueKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> _indexes;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> _parentKeys;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> _checks;
        private readonly AsyncLazy<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> _triggers;
    }
}
