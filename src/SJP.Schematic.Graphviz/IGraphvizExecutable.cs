using System;

namespace SJP.Schematic.Graphviz
{
    /// <summary>
    /// Defines a graphviz executable intended to call the dot executable in particular.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface IGraphvizExecutable : IDisposable
    {
        /// <summary>
        /// Retrieves a path to the dot executable.
        /// </summary>
        /// <value>A string that points to the dot executable.</value>
        string DotPath { get; }
    }
}
