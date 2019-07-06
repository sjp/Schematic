using System;
using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public class MigrationOperationResolverRegistry : IMigrationOperationResolverRegistry
    {
        public void AddResolver<TOperation>(IMigrationOperationResolver<TOperation> resolver) where TOperation : IMigrationOperation
        {
            _resolvers[typeof(TOperation)] = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public IMigrationOperationResolver<TOperation> GetResolver<TOperation>() where TOperation : IMigrationOperation
        {
            var opType = typeof(TOperation);
            if (!_resolvers.TryGetValue(opType, out var resolver))
                throw new KeyNotFoundException("No handler found for the given operation: " + opType.FullName);

            return (IMigrationOperationResolver<TOperation>)resolver;
        }

        private readonly IDictionary<Type, object> _resolvers = new Dictionary<Type, object>();
    }
}
