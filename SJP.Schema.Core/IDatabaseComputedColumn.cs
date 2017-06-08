using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseComputedColumn : IDatabaseColumn
    {
        ISqlExpression Expression { get; }

        IEnumerable<IDatabaseColumn> DependentColumns { get; }
    }
}
