using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameColumnOperation : MigrationOperation
    {
        public RenameColumnOperation(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Column = column ?? throw new ArgumentNullException(nameof(column));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseColumn Column { get; }

        public Identifier TargetName { get; }

        public override bool IsDestructive { get; } = true;
    }
}
