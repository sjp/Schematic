using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Defines generator for creating source code to work with a database entity.
/// </summary>
public interface IDatabaseEntityGenerator
{
    /// <summary>
    /// Gets the file path that the source code should be generated to.
    /// </summary>
    /// <param name="baseDirectory">The base directory.</param>
    /// <param name="objectName">The name of the database object.</param>
    /// <returns>A file path.</returns>
    IFileInfo GetFilePath(IDirectoryInfo baseDirectory, Identifier objectName);
}