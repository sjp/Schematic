using System.Collections.Generic;

namespace SJP.Schematic.Modelled.Reflection
{
    public interface IView
    {
        IReadOnlyDictionary<Dialect, string> Definition { get; }
    }
}
