using System.IO.Abstractions;

namespace SJP.Schematic.DataAccess
{
    public interface IDataAccessGenerator
    {
        void Generate(IFileSystem fileSystem, string projectPath, string baseNamespace);
    }
}
