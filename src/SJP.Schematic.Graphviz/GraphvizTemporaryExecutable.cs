using System.IO;

namespace SJP.Schematic.Graphviz
{
    public sealed class GraphvizTemporaryExecutable : IGraphvizExecutable
    {
        public GraphvizTemporaryExecutable()
        {
            _tempDir = new TemporaryDirectory();
            var zipResource = new ZippedResource(Resources.GraphVizZip);
            zipResource.ExtractToDirectory(_tempDir.DirectoryPath);
            DotPath = Path.Combine(_tempDir.DirectoryPath, "dot.exe");
            if (!File.Exists(DotPath))
                throw new FileNotFoundException($"Expected to find a file at: '{ DotPath }', but was not found.", DotPath);
        }

        public string DotPath { get; }

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
