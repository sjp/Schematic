using System;
using System.IO;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A temporary directory that will be deleted once disposed.
/// </summary>
/// <seealso cref="IDisposable" />
public sealed class TemporaryDirectory : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
    /// </summary>
    public TemporaryDirectory()
    {
        DirectoryPath = GetTempDirectoryPath();
        Directory.CreateDirectory(DirectoryPath);
    }

    /// <summary>
    /// The directory path of the temporary directory, always a random location.
    /// </summary>
    /// <value>The directory path.</value>
    public string DirectoryPath { get; }

    private static string GetTempDirectoryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName()
        );
    }

    /// <summary>
    /// Deletes the temporary directory, including all of its contents.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Directory.Delete(DirectoryPath, true);
        _disposed = true;
    }

    private bool _disposed;
}