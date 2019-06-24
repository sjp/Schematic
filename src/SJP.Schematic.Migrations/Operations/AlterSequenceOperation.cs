using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AlterSequenceOperation : MigrationOperation
    {
        public AlterSequenceOperation(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence)
        {
            ExistingSequence = existingSequence ?? throw new ArgumentNullException(nameof(existingSequence));
            TargetSequence = targetSequence ?? throw new ArgumentNullException(nameof(targetSequence));
        }

        public IDatabaseSequence ExistingSequence { get; }

        public IDatabaseSequence TargetSequence { get; }
    }
}
