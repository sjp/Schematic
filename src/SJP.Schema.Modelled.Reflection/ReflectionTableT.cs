using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionTable<T> : ReflectionTable where T : class, new()
    {
        public ReflectionTable(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
