using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Migrations
{
    public class MigrationError<TOperation> : IMigrationError
        where TOperation : class, IMigrationOperation
    {
        public MigrationError(string description, TOperation operation)
        {
            if (description.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(description));

            Description = description;
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public string Description { get; }

        public IMigrationOperation Operation { get; }
    }
}
