using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public sealed class MigrationAnalysisResult
    {
        private MigrationAnalysisResult(IEnumerable<IMigrationError> errors)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        private MigrationAnalysisResult(IEnumerable<IMigrationOperation> operations)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            Success = true;
        }

        public bool Success { get; }

        public IEnumerable<IMigrationError> Errors { get; } = Array.Empty<IMigrationError>();

        public IEnumerable<IMigrationOperation> Operations
        {
            get
            {
                if (!Success)
                    throw new InvalidOperationException("Unable to retrieve migration operations. There were errors during migration analysis.");

                return _operations;
            }
        }

        public IEnumerable<IMigrationOperation> GetOperationsUnsafe() => _operations;

        public static MigrationAnalysisResult Error(IEnumerable<IMigrationError> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            return new MigrationAnalysisResult(errors);
        }

        public static MigrationAnalysisResult Ok(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return new MigrationAnalysisResult(operations);
        }

        private readonly IEnumerable<IMigrationOperation> _operations = Array.Empty<IMigrationOperation>();
    }
}
