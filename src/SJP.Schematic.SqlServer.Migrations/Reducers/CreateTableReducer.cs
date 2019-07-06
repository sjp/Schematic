using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Reducers
{
    public class CreateTableReducer : IMigrationOperationReducer
    {
        public IEnumerable<IMigrationOperation> Reduce(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var createTableOperations = operations.OfType<CreateTableOperation>().ToList();
            var createTableNames = createTableOperations.Select(t => t.Table.Name).ToList();
            var result = new List<IMigrationOperation>();

            foreach (var operation in operations)
            {
                var isSkippedOperation = (operation is AddPrimaryKeyOperation pkOp && createTableNames.Contains(pkOp.Table.Name))
                    || (operation is AddTriggerOperation triggerOp && createTableNames.Contains(triggerOp.Table.Name))
                    || (operation is AddUniqueKeyOperation ukOp && createTableNames.Contains(ukOp.Table.Name))
                    || (operation is CreateIndexOperation indexOp && createTableNames.Contains(indexOp.Table.Name))
                    || (operation is AddForeignKeyOperation fkOp && createTableNames.Contains(fkOp.Table.Name))
                    || (operation is AddCheckOperation checkOp && createTableNames.Contains(checkOp.Table.Name));
                if (!isSkippedOperation)
                    result.Add(operation);
            }

            return result;
        }
    }
}
