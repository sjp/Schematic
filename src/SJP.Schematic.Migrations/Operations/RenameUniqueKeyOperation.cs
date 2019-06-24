using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameUniqueKeyOperation : MigrationOperation
    {
        public RenameUniqueKeyOperation(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            UniqueKey = uniqueKey ?? throw new ArgumentNullException(nameof(uniqueKey));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseKey UniqueKey { get; }

        public Identifier TargetName { get; }
    }
}
