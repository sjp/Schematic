using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class CreateSequenceOperation : MigrationOperation
    {
        public CreateSequenceOperation(IDatabaseSequence sequence)
        {
            Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
        }

        public IDatabaseSequence Sequence { get; }
    }
}
