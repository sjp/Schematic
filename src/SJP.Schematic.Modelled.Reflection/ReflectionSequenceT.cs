using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSequence<T> : ReflectionSequence where T : ISequence, new()
    {
        public ReflectionSequence(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
