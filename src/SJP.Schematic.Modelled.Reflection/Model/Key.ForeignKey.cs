using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        public abstract class ForeignKey : Key, IModelledRelationalKey
        {
            protected ForeignKey(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Foreign) { }

            public abstract Type TargetType { get; }

            public abstract Func<object, Key> KeySelector { get; }
        }
    }
}
