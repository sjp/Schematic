using System;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSynonym<T> : ReflectionSynonym where T : class, new()
    {
        public ReflectionSynonym(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
