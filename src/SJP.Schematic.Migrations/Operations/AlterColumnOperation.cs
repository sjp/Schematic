using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AlterColumnOperation : MigrationOperation
    {
        public AlterColumnOperation(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            ExistingColumn = existingColumn ?? throw new ArgumentNullException(nameof(existingColumn));
            TargetColumn = targetColumn ?? throw new ArgumentNullException(nameof(targetColumn));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseColumn ExistingColumn { get; }

        public IDatabaseColumn TargetColumn { get; }
    }
}
