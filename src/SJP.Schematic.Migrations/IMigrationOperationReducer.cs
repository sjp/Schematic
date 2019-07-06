using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    // removes duplicate operations
    // additionally removes any redundant operations, e.g. drop index and drop unique key when a drop table is about to happen
    public interface IMigrationOperationReducer
    {
        IEnumerable<IMigrationOperation> Reduce(IEnumerable<IMigrationOperation> operations);
    }
}
