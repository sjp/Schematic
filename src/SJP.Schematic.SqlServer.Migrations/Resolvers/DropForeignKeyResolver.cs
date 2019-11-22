using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Resolvers
{
    public class DropForeignKeyResolver : IMigrationOperationResolver<DropForeignKeyOperation>
    {
        public IAsyncEnumerable<IMigrationOperation> ResolveRequiredOperations(DropForeignKeyOperation operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return ResolveRequiredOperationsCore(operation);
        }

        private IAsyncEnumerable<IMigrationOperation> ResolveRequiredOperationsCore(DropForeignKeyOperation operation)
        {
            var tableParentKeys = operation.Table.ParentKeys;

            var hasExistingKey = tableParentKeys.Any(fk =>
                fk.ChildTable == operation.ForeignKey.ChildTable
                && fk.ParentTable == operation.ForeignKey.ParentTable
                && fk.ChildKey.Columns.Select(c => c.Name).SequenceEqual(
                    operation.ForeignKey.ChildKey.Columns.Select(c => c.Name))
                && fk.ParentKey.Columns.Select(c => c.Name).SequenceEqual(
                    operation.ForeignKey.ParentKey.Columns.Select(c => c.Name))
            );

            if (!hasExistingKey)
                return AsyncEnumerable.Empty<IMigrationOperation>();

            var fkColumnNames = operation.ForeignKey.ChildKey.Columns.Select(c => c.Name).ToList();
            var indexesToDrop = operation.Table.Indexes
                .Where(i => i.Columns.Any(c => c.DependentColumns.Any(dc => fkColumnNames.Contains(dc.Name)))
                    || i.IncludedColumns.Any(c => fkColumnNames.Contains(c.Name)))
                .ToList();
            var indexDropOperations = indexesToDrop.Select(i => new DropIndexOperation(operation.Table, i)).ToList();

            var result = new List<IMigrationOperation> { operation };
            result.AddRange(indexDropOperations);

            return result.ToAsyncEnumerable();
        }
    }
}
