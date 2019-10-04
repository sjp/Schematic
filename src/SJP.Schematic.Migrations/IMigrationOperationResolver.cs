using System.Collections.Generic;
using System.Threading;

namespace SJP.Schematic.Migrations
{
    // takes an intended operation and provides all of the required operations needed to satisfy it
    public interface IMigrationOperationResolver<TOperation> where TOperation : IMigrationOperation
    {
        IAsyncEnumerable<IMigrationOperation> ResolveRequiredOperations(TOperation operation, CancellationToken cancellationToken = default);
    }
}
