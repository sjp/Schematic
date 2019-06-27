using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameTriggerOperation : MigrationOperation
    {
        public RenameTriggerOperation(IRelationalDatabaseTable table, IDatabaseTrigger trigger, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseTrigger Trigger { get; }

        public Identifier TargetName { get; }
    }
}
