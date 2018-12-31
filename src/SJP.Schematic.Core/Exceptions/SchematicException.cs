using System;
using System.Runtime.Serialization;

namespace SJP.Schematic.Core.Exceptions
{
    [Serializable]
    public class SchematicException : Exception
    {
        public SchematicException() : base()
        {
        }

        public SchematicException(string message) : base(message)
        {
        }

        public SchematicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SchematicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
