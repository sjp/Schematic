using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseColumn
    {
        /// <summary>
        /// TODO: This may be null!! For example, a view where a column has no alias/name
        /// </summary>
        Identifier Name { get; }

        bool IsNullable { get; }

        bool IsComputed { get; }

        // retrieved from default constraint
        string DefaultValue { get; }

        IDbType Type { get; }

        bool IsAutoIncrement { get; }
    }
}
