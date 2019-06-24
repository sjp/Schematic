using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameSequenceOperation : MigrationOperation
    {
        public RenameSequenceOperation(IDatabaseSequence sequence, Identifier targetName)
        {
            Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IDatabaseSequence Sequence { get; }

        public Identifier TargetName { get; }
    }
}
