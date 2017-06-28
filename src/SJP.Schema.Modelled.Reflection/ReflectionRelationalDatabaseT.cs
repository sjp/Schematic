using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SJP.Schema.Core;
using SJP.Schema.Core.Utilities;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    // TODO: uncomment interface when ready
    public class ReflectionRelationalDatabase<T> : ReflectionRelationalDatabase //, IDependentRelationalDatabase
    {
        public ReflectionRelationalDatabase(IDatabaseDialect dialect, string databaseName = null, string defaultSchema = null)
            : base(dialect, typeof(T), databaseName, defaultSchema)
        {
        }
    }
}
