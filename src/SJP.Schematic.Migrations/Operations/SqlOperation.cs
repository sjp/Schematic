using System;

namespace SJP.Schematic.Migrations.Operations
{
    public class SqlOperation : MigrationOperation
    {
        public SqlOperation(ISqlCommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public ISqlCommand Command { get; }
    }
}
