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
            Message = "Unable to find the child table '"
                + (childTableName?.ToString() ?? string.Empty)
                + "' for the table '"
                + (tableName?.ToString() ?? string.Empty)
                + "'";

            TableName = tableName;
            ChildTableName = childTableName;
        }

        public Identifier TableName { get; }

        public Identifier ChildTableName { get; }

        public override string Message { get; }
    }
}
