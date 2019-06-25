using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationsSqlGenerator
    {
        Task<IReadOnlyList<ISqlCommand>> GenerateSql(IEnumerable<IMigrationOperation> operations);
    }
}
