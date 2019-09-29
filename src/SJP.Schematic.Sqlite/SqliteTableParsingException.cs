using System;
using System.Runtime.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Sqlite
{
    [Serializable]
    public class SqliteTableParsingException : SchematicException
    {
        public SqliteTableParsingException() : base()
        {
        }

        public SqliteTableParsingException(string message) : base(message)
        {
        }

        public SqliteTableParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqliteTableParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SqliteTableParsingException(Identifier tableName, string sql, string errorMessage)
        {
            Message = "Unable to parse the CREATE TABLE statement for the table '"
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
