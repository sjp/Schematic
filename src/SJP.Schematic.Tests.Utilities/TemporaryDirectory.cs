using System;
using System.IO;

namespace SJP.Schematic.Tests.Utilities
{
    public sealed class TemporaryDirectory : IDisposable
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

        public void Dispose()
        {
            if (_disposed)
                return;

            Directory.Delete(DirectoryPath, true);
            _disposed = true;
        }

        private bool _disposed;
    }
}
