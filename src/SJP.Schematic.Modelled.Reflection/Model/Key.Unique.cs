using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        public class Unique : Key
        {
            public Unique(params IModelledColumn[] columns)
                : this(columns as IEnumerable<IModelledColumn>)
            {
            }

            public Unique(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Unique)
            {
            }
        }
    }
}
