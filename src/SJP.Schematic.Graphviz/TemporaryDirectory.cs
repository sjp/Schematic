using System;
using System.IO;

namespace SJP.Schematic.Graphviz;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        DirectoryPath = GetTempDirectoryPath();
        Directory.CreateDirectory(DirectoryPath);
    }

    public string DirectoryPath { get; }

    private static string GetTempDirectoryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            Path.GetRandomFileName()
        );
    }

    private static void DeleteDirectory(string dirPath)
    {
        File.SetAttributes(dirPath, FileAttributes.Normal);

        foreach (var filePath in Directory.EnumerateFiles(dirPath))
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }

        foreach (var dir in Directory.GetDirectories(dirPath))
            DeleteDirectory(dir);

        Directory.Delete(dirPath, false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        DeleteDirectory(DirectoryPath);
        _disposed = true;
    }

    private bool _disposed;
}
