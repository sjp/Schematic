using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.DataAccess
{
    public interface IDataAccessGenerator
    {
        Task Generate(string projectPath, string baseNamespace, CancellationToken cancellationToken = default);
    }
}
