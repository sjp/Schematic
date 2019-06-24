using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameTableOperation : MigrationOperation
    {
        public RenameTableOperation(IRelationalDatabaseTable table, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public Identifier TargetName { get; }
    }
}
