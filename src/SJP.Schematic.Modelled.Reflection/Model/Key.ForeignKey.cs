using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        public abstract class ForeignKey : Key
        {
            protected ForeignKey(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Foreign) { }

            public abstract Type TargetType { get; }

            public abstract Func<object, Key> KeySelector { get; }
        }
    }
}
