using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public class MigrationOperationRegistry
    {
        public void AddAnalyzer<TOperation>(IMigrationOperationAnalyzer<TOperation> analyzer) where TOperation : IMigrationOperation
        {
            _analyzers[typeof(TOperation)] = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        }

        public IMigrationOperationAnalyzer<TOperation> GetAnalyzer<TOperation>() where TOperation : IMigrationOperation
        {
            var opType = typeof(TOperation);
            if (!_analyzers.TryGetValue(opType, out var analyzer))
                throw new KeyNotFoundException("No handler found for the given operation: " + opType.FullName);

            return (IMigrationOperationAnalyzer<TOperation>)analyzer;
        }

        private readonly IDictionary<Type, object> _analyzers = new Dictionary<Type, object>();
    }
}
