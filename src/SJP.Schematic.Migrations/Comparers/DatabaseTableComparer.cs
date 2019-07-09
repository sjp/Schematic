using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Comparers
{
    public class DatabaseTableComparer : IEqualityComparer<IRelationalDatabaseTable>
    {
        public DatabaseTableComparer(
            IEqualityComparer<IDatabaseCheckConstraint> checkComparer,
            IEqualityComparer<IDatabaseColumn> columnComparer,
            IEqualityComparer<IDatabaseIndex> indexComparer,
            IEqualityComparer<IDatabaseRelationalKey> relationalKeyComparer,
            IEqualityComparer<IDatabaseKey> keyComparer,
            IEqualityComparer<IDatabaseTrigger> triggerComparer
        )
        {
            CheckComparer = checkComparer ?? throw new ArgumentNullException(nameof(checkComparer));
            ColumnComparer = columnComparer ?? throw new ArgumentNullException(nameof(columnComparer));
            IndexComparer = indexComparer ?? throw new ArgumentNullException(nameof(indexComparer));
            RelationalKeyComparer = relationalKeyComparer ?? throw new ArgumentNullException(nameof(relationalKeyComparer));
            KeyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));
            TriggerComparer = triggerComparer ?? throw new ArgumentNullException(nameof(triggerComparer));
        }

        protected IEqualityComparer<IDatabaseCheckConstraint> CheckComparer { get; }

        protected IEqualityComparer<IDatabaseColumn> ColumnComparer { get; }

        protected IEqualityComparer<IDatabaseIndex> IndexComparer { get; }

        protected IEqualityComparer<IDatabaseRelationalKey> RelationalKeyComparer { get; }

        protected IEqualityComparer<IDatabaseKey> KeyComparer { get; }

        protected IEqualityComparer<IDatabaseTrigger> TriggerComparer { get; }

        public bool Equals(IRelationalDatabaseTable x, IRelationalDatabaseTable y)
        {
            if (x is null && y is null)
                return true;
            if (x is null ^ y is null)
                return false;

            var checksEqual = x.Checks.SequenceEqual(y.Checks, CheckComparer);
            var columnsEqual = x.Columns.SequenceEqual(y.Columns, ColumnComparer);
            var indexesEqual = x.Indexes.SequenceEqual(y.Indexes, IndexComparer);
            var foreignKeysEqual = x.ParentKeys.SequenceEqual(y.ParentKeys, RelationalKeyComparer);
            var primaryKeyEqual = x.PrimaryKey.Match(
                xpk => y.PrimaryKey.Match(ypk => KeyComparer.Equals(xpk, ypk), () => false),
                () => y.PrimaryKey.IsNone
            );
            var triggersEqual = x.Triggers.SequenceEqual(y.Triggers, TriggerComparer);
            var uniqueKeysEqual = x.UniqueKeys.SequenceEqual(y.UniqueKeys, KeyComparer);

            return x.Name == y.Name
                && checksEqual
                && columnsEqual
                && indexesEqual
                && foreignKeysEqual
                && primaryKeyEqual
                && triggersEqual
                && uniqueKeysEqual;
        }

        public int GetHashCode(IRelationalDatabaseTable obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Name);

            foreach (var check in obj.Checks)
                builder.Add(CheckComparer.GetHashCode(check));

            foreach (var column in obj.Columns)
                builder.Add(ColumnComparer.GetHashCode(column));

            foreach (var index in obj.Indexes)
                builder.Add(IndexComparer.GetHashCode(index));

            foreach (var foreignKey in obj.ParentKeys)
                builder.Add(RelationalKeyComparer.GetHashCode(foreignKey));

            var pkHashCode = obj.PrimaryKey.Match(
                pk => KeyComparer.GetHashCode(pk),
                () => 0
            );
            builder.Add(pkHashCode);

            foreach (var trigger in obj.Triggers)
                builder.Add(TriggerComparer.GetHashCode(trigger));

            foreach (var uniqueKey in obj.UniqueKeys)
                builder.Add(KeyComparer.GetHashCode(uniqueKey));

            return builder.ToHashCode();
        }
    }
}
