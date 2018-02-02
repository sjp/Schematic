using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseEntityGenerator
    {
        FileInfoBase GetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName);
    }
}
