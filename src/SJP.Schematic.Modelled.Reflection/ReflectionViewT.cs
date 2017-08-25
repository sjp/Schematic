using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionView<T> : ReflectionView where T : class, new()
    {
        public ReflectionView(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
