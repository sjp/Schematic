using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseIndex<T> where T : IDatabaseQueryable
    {
        T Parent { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseIndexColumn> Columns { get; }

        IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        bool IsUnique { get; }
    }
}
