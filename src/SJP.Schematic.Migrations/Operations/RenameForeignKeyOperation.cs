using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameForeignKeyOperation : MigrationOperation
    {
        public RenameForeignKeyOperation(IRelationalDatabaseTable childTable, IRelationalDatabaseTable parentTable, IDatabaseRelationalKey foreignKey, Identifier targetName)
        {
            ChildTable = childTable ?? throw new ArgumentNullException(nameof(childTable));
            ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
            ForeignKey = foreignKey ?? throw new ArgumentNullException(nameof(foreignKey));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable ChildTable { get; }

        public IRelationalDatabaseTable ParentTable { get; }

        public IDatabaseRelationalKey ForeignKey { get; }

        public Identifier TargetName { get; }
    }
}
