using System;
using System.Diagnostics.CodeAnalysis;

namespace SJP.Schematic.Core.Exceptions;

/// <summary>
/// An exception intended to be thrown trigger events are discovered that are not currently supported in Schematic.
/// </summary>
/// <seealso cref="SchematicException" />
[Serializable]
public class UnsupportedTriggerEventException : SchematicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedTriggerEventException"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public UnsupportedTriggerEventException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedTriggerEventException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    [ExcludeFromCodeCoverage]
    public UnsupportedTriggerEventException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedTriggerEventException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    [ExcludeFromCodeCoverage]
    public UnsupportedTriggerEventException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedTriggerEventException"/> class.
    /// </summary>
    /// <param name="tableName">The name of the table containing the trigger.</param>
    /// <param name="triggerEvent">The trigger event that is not supported.</param>
    public UnsupportedTriggerEventException(Identifier tableName, string triggerEvent)
    {
        TableName = tableName?.ToString() ?? string.Empty;
        TriggerEvent = triggerEvent;

        Message = "Found an unsupported trigger event name for a trigger on the table '"
            + TableName
            + "'. Expected one of INSERT, UPDATE, DELETE, got: "
            + triggerEvent;
    }

    /// <summary>
    /// Gets the name of the table that contains the unsupported trigger.
    /// </summary>
    /// <value>A table name.</value>
    public string TableName { get; } = string.Empty;

    /// <summary>
    /// Gets the trigger event that is not supported.
    /// </summary>
    /// <value>The trigger event name.</value>
    public string TriggerEvent { get; } = string.Empty;

    /// <summary>
    /// A message that describes the unsupported trigger event exception.
    /// </summary>
    public override string Message { get; } = string.Empty;
}