using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationOperationSorter
    {
        IEnumerable<IMigrationOperation> Sort(IEnumerable<IMigrationOperation> operations);
    }
}
