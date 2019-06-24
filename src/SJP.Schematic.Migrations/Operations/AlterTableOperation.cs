using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AlterTableOperation : MigrationOperation
    {
        public AlterTableOperation(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable)
        {
            ExistingTable = existingTable ?? throw new ArgumentNullException(nameof(existingTable));
            TargetTable = targetTable ?? throw new ArgumentNullException(nameof(targetTable));
        }

        public IRelationalDatabaseTable ExistingTable { get; }

        public IRelationalDatabaseTable TargetTable { get; }
    }
}
