using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Reducers
{
    public class DropTableReducer : IMigrationOperationReducer
    {
        public IEnumerable<IMigrationOperation> Reduce(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var dropTableOperations = operations.OfType<DropTableOperation>().ToList();
            var dropTableNames = dropTableOperations.Select(t => t.Table.Name).ToList();
            var result = new List<IMigrationOperation>();

            foreach (var operation in operations)
            {
                var isSkippedOperation = (operation is DropPrimaryKeyOperation pkOp && dropTableNames.Contains(pkOp.Table.Name))
                    || (operation is DropTriggerOperation triggerOp && dropTableNames.Contains(triggerOp.Table.Name))
                    || (operation is DropUniqueKeyOperation ukOp && dropTableNames.Contains(ukOp.Table.Name))
                    || (operation is DropCheckOperation checkOp && dropTableNames.Contains(checkOp.Table.Name));
                if (!isSkippedOperation)
                    result.Add(operation);
            }

            return result;
        }
    }
}
