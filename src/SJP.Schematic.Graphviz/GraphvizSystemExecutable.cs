using System;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// A Graphviz executable that is available on the system.
/// </summary>
/// <seealso cref="IGraphvizExecutable" />
public sealed class GraphvizSystemExecutable : IGraphvizExecutable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizSystemExecutable"/> class.
    /// </summary>
    /// <param name="dotExecutablePath">A dot executable path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dotExecutablePath"/> is <see langword="null" />, empty or whitespace.</exception>
    public GraphvizSystemExecutable(string dotExecutablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dotExecutablePath);

        DotPath = dotExecutablePath;
    }

    /// <summary>
    /// The path to the dot executable.
    /// </summary>
    /// <value>A string representing a path or executable name available in the <c>PATH</c> environment variable.</value>
    public string DotPath { get; }

    /// <summary>
    /// Intended to release resources, for this particular implementation it does nothing.
    /// </summary>
    public void Dispose()
    {
        // nothing to do here, only required to make shared interface cleaner
    }
}