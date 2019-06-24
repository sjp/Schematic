using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameIndexOperation : MigrationOperation
    {
        public RenameIndexOperation(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Index = index ?? throw new ArgumentNullException(nameof(index));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseIndex Index { get; }

        public Identifier TargetName { get; }
    }
}
