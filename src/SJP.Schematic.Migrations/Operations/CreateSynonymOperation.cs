using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class CreateSynonymOperation : MigrationOperation
    {
        public CreateSynonymOperation(IDatabaseSynonym synonym)
        {
            Synonym = synonym ?? throw new ArgumentNullException(nameof(synonym));
        }

        public IDatabaseSynonym Synonym { get; }
    }
}
