using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Resolvers
{
    public class AddCheckResolver : IMigrationOperationResolver<AddCheckOperation>
    {
        public Task<IReadOnlyCollection<IMigrationOperation>> ResolveRequiredOperations(AddCheckOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            return ResolveRequiredOperationsCore(operation);
        }

        public Task<IReadOnlyCollection<IMigrationOperation>> ResolveRequiredOperationsCore(AddCheckOperation operation)
        {
            var tableChecks = operation.Table.Checks;
            var hasExistingDefinition = tableChecks.Any(c => c.Definition == operation.Check.Definition);

            var hasCheckByName = operation.Check.Name.Match(
                opCheckName => tableChecks.Any(check => check.Name.Match(
                    checkName => checkName.LocalName == opCheckName.LocalName,
                    () => false)),
                () => false);

            // TODO throw if hasCheckByName is true
            var result = hasExistingDefinition || hasCheckByName
                ? Array.Empty<IMigrationOperation>()
                : new[] { operation } as IReadOnlyCollection<IMigrationOperation>;

            return Task.FromResult(result);
        }
    }
}
