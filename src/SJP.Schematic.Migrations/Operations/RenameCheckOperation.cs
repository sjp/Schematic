using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameCheckOperation : MigrationOperation
    {
        public RenameCheckOperation(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Check = check ?? throw new ArgumentNullException(nameof(check));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseCheckConstraint Check { get; }

        public Identifier TargetName { get; }
    }
}
