using System;

namespace SJP.Schematic.Core.Utilities
{
    public interface IVersionedLookup<T>
    {
        T GetValue(Version version);
    }
}
