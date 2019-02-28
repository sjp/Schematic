using System;
using System.IO;

namespace SJP.Schematic.Graphviz
{
    public class GraphvizTemporaryExecutable : IDisposable
    {
        public GraphvizTemporaryExecutable()
        {
            _tempDir = new TemporaryDirectory();
            var zipResource = new ZippedResource(Resources.GraphVizZip);
            zipResource.ExtractToDirectory(_tempDir.DirectoryPath);
            DotExecutablePath = Path.Combine(_tempDir.DirectoryPath, "dot.exe");
            if (!File.Exists(DotExecutablePath))
                throw new FileNotFoundException($"Expected to find a file at: '{ DotExecutablePath }', but was not found.", DotExecutablePath);
        }

        public string DotExecutablePath { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            _tempDir.Dispose();
            _disposed = true;
        }

        private readonly TemporaryDirectory _tempDir;
        private bool _disposed;
    }
}
