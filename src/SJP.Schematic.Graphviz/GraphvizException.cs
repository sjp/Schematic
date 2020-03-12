using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SJP.Schematic.Graphviz
{
    public class GraphvizException : Exception
    {
        public GraphvizException(int exitCode, string errorMessage)
            : base(errorMessage)
        {
            ExitCode = exitCode;
        }

        [ExcludeFromCodeCoverage]
        public GraphvizException()
        {
        }

        [ExcludeFromCodeCoverage]
        public GraphvizException(string message) : base(message)
        {
        }

        [ExcludeFromCodeCoverage]
        public GraphvizException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [ExcludeFromCodeCoverage]
        protected GraphvizException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public int ExitCode { get; }
    }
}
