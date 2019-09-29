using System;
using System.Runtime.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Sqlite
{
    [Serializable]
    public class SqliteTriggerParsingException : SchematicException
    {
        public SqliteTriggerParsingException() : base()
        {
        }

        public SqliteTriggerParsingException(string message) : base(message)
        {
        }

        public SqliteTriggerParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
