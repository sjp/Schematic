using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledColumn
    {
        PropertyInfo Property { get; }

        bool IsNullable { get; }

        Type DeclaredDbType { get; }
    }
}
