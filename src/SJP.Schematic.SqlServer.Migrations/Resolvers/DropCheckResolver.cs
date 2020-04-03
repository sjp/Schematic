using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Resolvers
{
    public class DropCheckResolver : IMigrationOperationResolver<DropCheckOperation>
    {
        public IAsyncEnumerable<IMigrationOperation> ResolveRequiredOperations(DropCheckOperation operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return ResolveRequiredOperationsCore(operation);
        }

        private IAsyncEnumerable<IMigrationOperation> ResolveRequiredOperationsCore(DropCheckOperation operation)
        {
            var tableChecks = operation.Table.Checks;
            var hasExistingDefinition = tableChecks.Any(c => c.Definition == operation.Check.Definition);

            var hasCheckByName = operation.Check.Name.Match(
                opCheckName => tableChecks.Any(check => check.Name.Match(
                    checkName => checkName.LocalName == opCheckName.LocalName,
                    () => false)),
                () => false);

            // TODO throw if hasCheckByName is false
            var result = !hasExistingDefinition && !hasCheckByName
                ? Array.Empty<IMigrationOperation>()
                : new IMigrationOperation[] { operation };

            return result.ToAsyncEnumerable();
        }
    }
}
