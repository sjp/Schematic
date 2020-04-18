using System;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Graphviz
{
    public sealed class GraphvizSystemExecutable : IGraphvizExecutable
    {
        public GraphvizSystemExecutable(string dotExecutablePath)
        {
            if (dotExecutablePath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(dotExecutablePath));

            DotPath = dotExecutablePath;
        }

        public string DotPath { get; }

        public void Dispose()
        {
            // nothing to do here, only required to make shared interface cleaner
        }
    }
}
