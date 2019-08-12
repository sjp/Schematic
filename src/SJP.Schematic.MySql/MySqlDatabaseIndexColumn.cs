using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public MySqlDatabaseIndexColumn(string expression, IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
            DependentColumns = new[] { column };
        }

        public string Expression { get; }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }
    }
}
