using System;
using System.Collections.Generic;
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
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        public IDatabaseKey PrimaryKey { get; }

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(PrimaryKey);

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Indexes);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(UniqueKeys);

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(ChildKeys);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Checks);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(ParentKeys);

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Columns);

        public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Triggers);
    }
}
