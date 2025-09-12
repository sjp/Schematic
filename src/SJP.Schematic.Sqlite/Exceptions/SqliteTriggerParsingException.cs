using System;
using System.Diagnostics.CodeAnalysis;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Sqlite.Exceptions;

/// <summary>
/// An exception thrown when Schematic is unable to parse a SQLite trigger definition.
/// </summary>
/// <seealso cref="SchematicException" />
[Serializable]
public class SqliteTriggerParsingException : SchematicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTriggerParsingException"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public SqliteTriggerParsingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTriggerParsingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    [ExcludeFromCodeCoverage]
    public SqliteTriggerParsingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTriggerParsingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null" /> if no inner exception is specified.</param>
    [ExcludeFromCodeCoverage]
    public SqliteTriggerParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTriggerParsingException"/> class.
    /// </summary>
    /// <param name="tableName">The table containing the trigger.</param>
    /// <param name="sql">The SQL which was unable to be parsed.</param>
    /// <param name="errorMessage">An error message.</param>
    public SqliteTriggerParsingException(Identifier tableName, string sql, string errorMessage)
    {
        Message = "Unable to parse the TRIGGER statement for the table '"
            + (tableName?.ToString() ?? string.Empty)
            + "'.";

        TableName = tableName;
        Sql = sql;
        ParsingErrorMessage = errorMessage;
    }

    /// <summary>
    /// The table containing the trigger.
    /// </summary>
    /// <value>A table name.</value>
    public Identifier? TableName { get; }

    /// <summary>
    /// The SQL which was unable to be parsed.
    /// </summary>
    /// <value>A trigger definition.</value>
    public string? Sql { get; }

    /// <summary>
    /// The parsing error message.
    /// </summary>
    /// <value>An error message.</value>
    public string? ParsingErrorMessage { get; }

    /// <summary>
    /// Gets a message that describes the current exception.
    /// </summary>
    public override string Message { get; } = string.Empty;
}