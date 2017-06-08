using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseCheckConstraint
    {
        Identifier Name { get; }

        IRelationalDatabaseTable Table { get; }

        // something similar to Index -- need to find a generic way to build expression-like objects
        // something like var x = new Index(a, b) for a, b, both ascending
        // Index(a, b => b.Descending) for a asc, b desc
        // Index(a, b => new Expression("LOWER({0})", b))
        ISqlExpression Expression { get; }

        IEnumerable<IDatabaseColumn> DependentColumns { get; }
    }
}
