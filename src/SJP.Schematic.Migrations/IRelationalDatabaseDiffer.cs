using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public interface IRelationalDatabaseDiffer
    {
        IEnumerable<IMigrationOperation> Compare(IRelationalDatabaseDiffer comparison);
    }
}
