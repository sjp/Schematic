using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    public interface IDatabaseEntity
    {
        Identifier Name { get; }

        // TODO! I think this should be implemented...
        // Maybe push into Async?
        //IEnumerable<Identifier> Dependencies { get; }
        //
        //Task<IEnumerable<Identifier>> DependenciesAsync();
        //
        //IEnumerable<Identifier> Dependents { get; }
        //
        //Task<IEnumerable<Identifier>> DependentsAsync();
    }
}
