using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Removes source files left behind in a generated project by previous generation runs.
/// </summary>
public static class ProjectFileCleaner
{
    /// <summary>
    /// Deletes every C# source file within a generated project directory that was not written by the current generation run.
    /// </summary>
    /// <param name="projectDirectory">The directory containing the generated project.</param>
    /// <param name="generatedFilePaths">The paths of the files written by the current generation run.</param>
    /// <exception cref="ArgumentNullException"><paramref name="projectDirectory"/> or <paramref name="generatedFilePaths"/> is <see langword="null" />.</exception>
    public static void RemoveStaleFiles(IDirectoryInfo projectDirectory, IEnumerable<string> generatedFilePaths)
    {
        ArgumentNullException.ThrowIfNull(projectDirectory);
        ArgumentNullException.ThrowIfNull(generatedFilePaths);

        if (!projectDirectory.Exists)
            return;

        var generatedFiles = new HashSet<string>(generatedFilePaths, StringComparer.OrdinalIgnoreCase);

        // Materialized rather than enumerated lazily so that deleting does not disturb the search.
        var existingFiles = projectDirectory.GetFiles("*.cs", SearchOption.AllDirectories);
        foreach (var existingFile in existingFiles)
        {
            if (!generatedFiles.Contains(existingFile.FullName))
                existingFile.Delete();
        }
    }
}
