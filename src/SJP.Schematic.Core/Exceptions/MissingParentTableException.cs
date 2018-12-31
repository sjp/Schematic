using System;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions
{
    [Serializable]
    public class MissingParentTableException : SchematicException
    {
        public MissingParentTableException()
        {
        }

        public MissingParentTableException(string message) : base(message)
        {
        }

        public MissingParentTableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingParentTableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingParentTableException(Identifier tableName, Identifier parentTableName)
        {
            Message = "Unable to find the parent table '"
                + (parentTableName?.ToString() ?? string.Empty)
                + "' for the table '"
                + (tableName?.ToString() ?? string.Empty)
                + "'";

            TableName = tableName;
            ParentTableName = parentTableName;
        }

        public Identifier TableName { get; }

        public Identifier ParentTableName { get; }

        public override string Message { get; }
    }
}
