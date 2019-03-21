using System;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions
{
    [Serializable]
    public class MissingChildTableException : SchematicException
    {
        public MissingChildTableException()
        {
        }

        public MissingChildTableException(string message) : base(message)
        {
        }

        public MissingChildTableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingChildTableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingChildTableException(Identifier tableName, Identifier childTableName)
        {
            ChildTableName = childTableName?.ToString() ?? string.Empty;
            TableName = tableName?.ToString() ?? string.Empty;

            Message = "Unable to find the child table '"
                + ChildTableName
                + "' for the table '"
                + TableName
                + "'";
        }

        public string TableName { get; }

        public string ChildTableName { get; }

        public override string Message { get; }
    }
}
