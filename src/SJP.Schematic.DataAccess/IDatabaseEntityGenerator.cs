using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseEntityGenerator
    {
        FileInfo GetFilePath(DirectoryInfo baseDirectory, Identifier objectName);
    }
}
