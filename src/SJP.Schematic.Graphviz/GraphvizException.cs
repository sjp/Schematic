using System;
using System.Diagnostics.CodeAnalysis;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// An exception intended to be thrown when there is an error thrown when executing a Graphviz process.
/// </summary>
/// <seealso cref="Exception" />
[Serializable]
public class GraphvizException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizException"/> class.
    /// </summary>
    /// <param name="exitCode">An exit code for the graphviz process.</param>
    /// <param name="errorMessage">An error message.</param>
    public GraphvizException(int exitCode, string errorMessage)
        : base(errorMessage)
    {
        ExitCode = exitCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizException"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public GraphvizException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    [ExcludeFromCodeCoverage]
    public GraphvizException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null" /> reference if no inner exception is specified.</param>
    [ExcludeFromCodeCoverage]
    public GraphvizException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// The exit code of the graphviz process.
    /// </summary>
    /// <value>An exit code.</value>
    public int ExitCode { get; }
}