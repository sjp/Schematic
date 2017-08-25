using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSequence<T> : ReflectionSequence where T : ISequence, new()
    {
        public ReflectionSequence(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
