using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenamePrimaryKeyOperation : MigrationOperation
    {
        public RenamePrimaryKeyOperation(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            PrimaryKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseKey PrimaryKey { get; }

        public Identifier TargetName { get; }
    }
}
