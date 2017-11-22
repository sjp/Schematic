using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledRelationalKey : IModelledKey
    {
        Type TargetType { get; }

        Func<object, Key> KeySelector { get; }
    }
}
