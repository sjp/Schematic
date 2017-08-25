using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        protected Key(IEnumerable<IModelledColumn> columns, DatabaseKeyType keyType)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Columns = columns;
            KeyType = keyType;
        }

        public IEnumerable<IModelledColumn> Columns { get; }

        public DatabaseKeyType KeyType { get; }

        public PropertyInfo Property { get; set; }
    }
}
