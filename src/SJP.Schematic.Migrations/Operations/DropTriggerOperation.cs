using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropTriggerOperation : MigrationOperation
    {
        public DropTriggerOperation(IRelationalDatabaseTable table, IDatabaseTrigger trigger)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseTrigger Trigger { get; }
    }
}
