using System.IO;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// A graphviz executable that is available temporarily, and applies only for Windows.
/// </summary>
/// <seealso cref="IGraphvizExecutable" />
public sealed class GraphvizTemporaryExecutable : IGraphvizExecutable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizTemporaryExecutable"/> class.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when the temporary executable was unable to be extracted successfully.</exception>
    public GraphvizTemporaryExecutable()
    {
        _tempDir = new TemporaryDirectory();
        var zipResource = new ZippedResource(Resources.GraphVizZip);
        zipResource.ExtractToDirectory(_tempDir.DirectoryPath);
        DotPath = Path.Combine(_tempDir.DirectoryPath, "dot.exe");
        if (!File.Exists(DotPath))
            throw new FileNotFoundException($"Expected to find a file at: '{ DotPath }', but was not found.", DotPath);
    }

    /// <summary>
    /// A path to the dot executable.
    /// </summary>
    /// <value>A string representing a path to the dot executable.</value>
    public string DotPath { get; }

    /// <summary>
    /// Removes the temporary executable and associated files.
    /// </summary>
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