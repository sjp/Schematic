using System;

namespace SJP.Schematic.Graphviz
{
    public interface IGraphvizExecutable : IDisposable
    {
        string DotPath { get; }
    }
}
