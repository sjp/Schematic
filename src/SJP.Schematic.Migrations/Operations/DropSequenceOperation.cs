using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropSequenceOperation : MigrationOperation
    {
        public DropSequenceOperation(IDatabaseSequence sequence)
        {
            Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public IDatabaseSequence Sequence { get; }
    }
}
