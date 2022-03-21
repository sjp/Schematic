using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions;

/// <summary>
/// A base class to be used for building exceptions for use within Schematic.
/// </summary>
/// <seealso cref="Exception" />
[Serializable]
[ExcludeFromCodeCoverage]
public class SchematicException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicException"/> class.
    /// </summary>
    public SchematicException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SchematicException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public SchematicException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    protected SchematicException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}