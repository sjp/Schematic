using System;

namespace SJP.Schematic.Graphviz
{
    public class GraphvizException : Exception
    {
        public GraphvizException(int exitCode, string errorMessage)
            : base(errorMessage)
        {
            ExitCode = exitCode;
        }

        public GraphvizException()
        {
        }

        public GraphvizException(string message) : base(message)
        {
        }

        public GraphvizException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public int ExitCode { get; }
    }
}
