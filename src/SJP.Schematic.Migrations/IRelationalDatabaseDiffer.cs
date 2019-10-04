using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations
{
    public interface IRelationalDatabaseDiffer
    {
        IAsyncEnumerable<IMigrationOperation> GetDifferences(IRelationalDatabase comparison, CancellationToken cancellationToken = default);
    }
}
