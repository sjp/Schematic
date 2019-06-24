using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropSynonymOperation : MigrationOperation
    {
        public DropSynonymOperation(IDatabaseSynonym synonym)
        {
            Synonym = synonym ?? throw new ArgumentNullException(nameof(synonym));
        }

        public IDatabaseSynonym Synonym { get; }
    }
}
