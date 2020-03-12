using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Sqlite.Exceptions
{
    [Serializable]
    public class SqliteTriggerParsingException : SchematicException
    {
        [ExcludeFromCodeCoverage]
        public SqliteTriggerParsingException()
        {
        }

        [ExcludeFromCodeCoverage]
        public SqliteTriggerParsingException(string message) : base(message)
        {
        }

        [ExcludeFromCodeCoverage]
        public SqliteTriggerParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [ExcludeFromCodeCoverage]
        protected SqliteTriggerParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SqliteTriggerParsingException(Identifier tableName, string sql, string errorMessage)
        {
            Message = "Unable to parse the TRIGGER statement for the table '"
                + (tableName?.ToString() ?? string.Empty)
                + "'.";

            TableName = tableName;
            Sql = sql;
            ParsingErrorMessage = errorMessage;
        }

        public Identifier? TableName { get; }

        public string? Sql { get; }

        public string? ParsingErrorMessage { get; }

        public override string Message { get; } = string.Empty;
    }
}
