using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionView<T> : ReflectionView where T : class, new()
    {
        public ReflectionView(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
