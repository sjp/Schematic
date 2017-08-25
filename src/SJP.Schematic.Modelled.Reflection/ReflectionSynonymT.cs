using System;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSynonym<T> : ReflectionSynonym where T : class, new()
    {
        public ReflectionSynonym(IRelationalDatabase database)
            : base(database, typeof(T))
        {
        }
    }
}
