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
            ParentTableName = parentTableName?.ToString() ?? string.Empty;
            TableName = tableName?.ToString() ?? string.Empty;

            Message = "Unable to find the parent table '"
                + ParentTableName
                + "' for the table '"
                + TableName
                + "'";
        }

        public string TableName { get; }

        public string ParentTableName { get; }

        public override string Message { get; }
    }
}
