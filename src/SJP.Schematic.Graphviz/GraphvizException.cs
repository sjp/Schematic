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

        public int ExitCode { get; }
    }
}
