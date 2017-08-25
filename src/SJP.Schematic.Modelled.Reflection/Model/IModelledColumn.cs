using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledColumn
    {
        Type DeclaredDbType { get; }

        bool IsComputed { get; }

        bool IsNullable { get; }

        PropertyInfo Property { get; }
    }
}
