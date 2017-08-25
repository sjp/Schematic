using System.Collections.Generic;

namespace SJP.Schema.Modelled.Reflection
{
    public interface IView
    {
        IReadOnlyDictionary<Dialect, string> Definition { get; }
    }
}
