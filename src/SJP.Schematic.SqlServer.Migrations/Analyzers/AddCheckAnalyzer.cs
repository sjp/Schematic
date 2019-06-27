using System;
using LanguageExt;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations.Analyzers
{
    public class AddCheckAnalyzer : IMigrationOperationAnalyzer<AddCheckOperation>
    {
        public EitherAsync<IMigrationError, IMigrationOperation> GetDependentOperations(AddCheckOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            // TODO: implement...
            return EitherAsync<IMigrationError, IMigrationOperation>.Right(null);
        }
    }
}
