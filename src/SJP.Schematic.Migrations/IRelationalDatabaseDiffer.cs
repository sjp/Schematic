using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations
{
    public interface IRelationalDatabaseDiffer
    {
        Task<bool> HasDifferences(IRelationalDatabase comparison, CancellationToken cancellationToken = default(CancellationToken));

        Task<MigrationAnalysisResult> GetDifferences(IRelationalDatabase comparison, CancellationToken cancellationToken = default(CancellationToken));
    }
}
