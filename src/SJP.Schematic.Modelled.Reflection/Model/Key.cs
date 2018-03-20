using System;
using System.Collections.Generic;
using System.Reflection;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        protected Key(IEnumerable<IModelledColumn> columns, DatabaseKeyType keyType)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            Columns = columns;
            KeyType = keyType;
        }

        public IEnumerable<IModelledColumn> Columns { get; }

        public DatabaseKeyType KeyType { get; }

        public PropertyInfo Property { get; set; }
    }
}
