using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropTableOperation : MigrationOperation
    {
        public DropTableOperation(IRelationalDatabaseTable table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }

        public override bool IsDestructive { get; } = true;
    }
}
