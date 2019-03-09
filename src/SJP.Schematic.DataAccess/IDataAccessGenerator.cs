using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.DataAccess
{
    public interface IDataAccessGenerator
    {
        void Generate(string projectPath, string baseNamespace);

        Task GenerateAsync(string projectPath, string baseNamespace, CancellationToken cancellationToken = default(CancellationToken));
    }
}
