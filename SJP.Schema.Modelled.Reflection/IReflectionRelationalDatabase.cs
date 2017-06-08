using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    // TODO: this looks nasty!
    // why does a consumer need to know?
    // maybe just make this implementation specific on the class?
    public interface IReflectionRelationalDatabase : IRelationalDatabase
    {
        IReadOnlyDictionary<Type, object> TableInstances { get; }

        IReadOnlyDictionary<Type, object> ViewInstances { get; }

        IReadOnlyDictionary<Type, object> SequenceInstances { get; }
    }
}
