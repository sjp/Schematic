using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Migrations
{
    public interface ISqlGenerator<TOperation> where TOperation : IMigrationOperation
    {
        Task<IReadOnlyList<ISqlCommand>> GenerateSql(TOperation operation);
    }
}
