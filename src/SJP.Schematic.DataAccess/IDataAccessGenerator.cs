using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.DataAccess
{
    public interface IDataAccessGenerator
    {
        void Generate(IFileSystem fileSystem, string projectPath, string baseNamespace);

        Task GenerateAsync(IFileSystem fileSystem, string projectPath, string baseNamespace, CancellationToken cancellationToken = default(CancellationToken));
    }
}
