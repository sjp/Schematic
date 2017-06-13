using System.Collections.Generic;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledKey
    {
        PropertyInfo Property { get; set; }

        DatabaseKeyType KeyType { get; }

        IEnumerable<IModelledColumn> Columns { get; }
    }
}
