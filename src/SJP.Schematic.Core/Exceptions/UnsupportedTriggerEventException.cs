using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions
{
    [Serializable]
    public class UnsupportedTriggerEventException : SchematicException
    {
        [ExcludeFromCodeCoverage]
        public UnsupportedTriggerEventException()
        {
        }

        [ExcludeFromCodeCoverage]
        public UnsupportedTriggerEventException(string message) : base(message)
        {
        }

        [ExcludeFromCodeCoverage]
        public UnsupportedTriggerEventException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [ExcludeFromCodeCoverage]
        protected UnsupportedTriggerEventException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedTriggerEventException(Identifier tableName, string triggerEvent)
        {
            TableName = tableName?.ToString() ?? string.Empty;
            TriggerEvent = triggerEvent;

            Message = "Found an unsupported trigger event name for a trigger on the table '"
                + TableName
                + "'. Expected one of INSERT, UPDATE, DELETE, got: "
                + triggerEvent;
        }

        public string TableName { get; } = string.Empty;

        public string TriggerEvent { get; } = string.Empty;

        public override string Message { get; } = string.Empty;
    }
}
