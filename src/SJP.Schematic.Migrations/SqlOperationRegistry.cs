using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public class SqlOperationRegistry
    {
        public void AddGenerator<TOperation>(ISqlGenerator<TOperation> generator) where TOperation : IMigrationOperation
        {
            _sqlGenerators[typeof(TOperation)] = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public ISqlGenerator<TOperation> GetGenerator<TOperation>() where TOperation : IMigrationOperation
        {
            var opType = typeof(TOperation);
            if (!_sqlGenerators.TryGetValue(opType, out var generator))
                throw new KeyNotFoundException("No handler found for the given operation: " + opType.FullName);

            return (ISqlGenerator<TOperation>)generator;
        }

        private readonly IDictionary<Type, object> _sqlGenerators = new Dictionary<Type, object>();
    }
}
