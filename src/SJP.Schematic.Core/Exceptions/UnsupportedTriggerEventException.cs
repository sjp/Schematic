using System;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions
{
    [Serializable]
    public class UnsupportedTriggerEventException : SchematicException
    {
        public UnsupportedTriggerEventException()
        {
        }

        public UnsupportedTriggerEventException(string message) : base(message)
        {
        }

        public UnsupportedTriggerEventException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedTriggerEventException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedTriggerEventException(Identifier tableName, string triggerEvent)
        {
            Message = "Found an unsupported trigger event name for a trigger on the table '"
                + (tableName?.ToString() ?? string.Empty)
                + "'. Expected one of INSERT, UPDATE, DELETE, got: "
                + triggerEvent;

            TableName = tableName;
        }

        public Identifier TableName { get; }

        public string TriggerEvent { get; }

        public override string Message { get; }
    }
}
