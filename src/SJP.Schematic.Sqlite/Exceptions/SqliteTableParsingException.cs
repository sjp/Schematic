using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Sqlite.Exceptions;

/// <summary>
/// An exception that is intended to be thrown when a <c>CREATE TABLE</c> definition is unable to be parsed.
/// </summary>
/// <seealso cref="SchematicException" />
[Serializable]
public class SqliteTableParsingException : SchematicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableParsingException"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public SqliteTableParsingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableParsingException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    [ExcludeFromCodeCoverage]
    public SqliteTableParsingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableParsingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    [ExcludeFromCodeCoverage]
    public SqliteTableParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableParsingException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
    [ExcludeFromCodeCoverage]
    protected SqliteTableParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableParsingException"/> class.
    /// </summary>
    /// <param name="tableName">The table containing the trigger.</param>
    /// <param name="sql">The SQL which was unable to be parsed.</param>
    /// <param name="errorMessage">An error message.</param>
    public SqliteTableParsingException(Identifier tableName, string sql, string errorMessage)
    {
        Message = "Unable to parse the CREATE TABLE statement for the table '"
            + (tableName?.ToString() ?? string.Empty)
            + "'.";

        TableName = tableName;
        Sql = sql;
        ParsingErrorMessage = errorMessage;
    }

    /// <summary>
    /// The table that was unable to be parsed.
    /// </summary>
    /// <value>The name of the table.</value>
    public Identifier? TableName { get; }

    /// <summary>
    /// The SQL which was unable to be parsed.
    /// </summary>
    /// <value>A table definition.</value>
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
