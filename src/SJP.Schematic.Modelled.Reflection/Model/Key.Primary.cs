using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
        public class Primary : Key
        {
            public Primary(params IModelledColumn[] columns)
                : this(columns as IEnumerable<IModelledColumn>)
            {
            }

            public Primary(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Primary)

            {
                if (columns == null)
                    throw new ArgumentNullException(nameof(columns));

                var nullableColumns = columns
                    .Where(static c => c.IsNullable)
                    .Select(static c => c.Property!.Name)
                    .ToList();

                if (nullableColumns.Count > 0)
                    throw new ArgumentException("Nullable columns cannot be members of a primary key. Nullable column properties: " + nullableColumns.Join(", "), nameof(columns));
            }
        }
    }
}
