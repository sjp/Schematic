using System.Collections.Generic;
using System.Reflection;

namespace SJP.Schema.Modelled.Reflection
{
    public interface IModelledIndex
    {
        PropertyInfo Property { get; set; }

        bool IsUnique { get; }

        IEnumerable<IModelledIndexColumn> Columns { get; }

        IEnumerable<IModelledColumn> IncludedColumns { get; }
    }
}
