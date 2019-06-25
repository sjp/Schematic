using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Migrations
{
    public interface IRelationalDatabaseDiffer
    {
        Task<IEnumerable<IMigrationOperation>> Compare(IRelationalDatabaseDiffer comparison);
    }
}
