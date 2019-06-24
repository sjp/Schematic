using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropColumnOperation : MigrationOperation
    {
        public DropColumnOperation(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseColumn Column { get; }

        public override bool IsDestructive { get; } = true;
    }
}
