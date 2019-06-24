using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameSynonymOperation : MigrationOperation
    {
        public RenameSynonymOperation(IDatabaseSynonym synonym, Identifier targetName)
        {
            Synonym = synonym ?? throw new ArgumentNullException(nameof(synonym));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IDatabaseSynonym Synonym { get; }

        public Identifier TargetName { get; }
    }
}
