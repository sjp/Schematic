using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Migrations
{

    public interface ISqlGenerator<T> where T : IMigrationOperation
    {
        Task<IReadOnlyList<ISqlCommand>> GenerateSql(T operation);
    }
}
