using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AddCheckOperation : MigrationOperation
    {
        public AddCheckOperation(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Check = check ?? throw new ArgumentNullException(nameof(check));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseCheckConstraint Check { get; }
    }
}
