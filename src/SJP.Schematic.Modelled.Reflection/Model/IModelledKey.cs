using System.Collections.Generic;
using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledKey
    {
        PropertyInfo Property { get; set; }

        DatabaseKeyType KeyType { get; }

        IEnumerable<IModelledColumn> Columns { get; }
    }
}
