using System;
using System.Reflection;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledColumn
    {
        Type DeclaredDbType { get; }

        bool IsComputed { get; }

        bool IsNullable { get; }

        PropertyInfo Property { get; }
    }
}
