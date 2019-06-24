using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropIndexOperation : MigrationOperation
    {
        public DropIndexOperation(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Index = index ?? throw new ArgumentNullException(nameof(index));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseIndex Index { get; }
    }
}
