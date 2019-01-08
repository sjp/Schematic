using System;

namespace SJP.Schematic.Core.Utilities
{
    public interface IVersionedLookup<out T>
    {
        T GetValue(Version version);
    }
}
