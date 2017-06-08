using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    // TODO!

    public class SqlServerDatabaseComputedTableColumn : SqlServerDatabaseTableColumn, IDatabaseTableColumn, IDatabaseComputedColumn
    {
        public SqlServerDatabaseComputedTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue)
            : base(table, columnName, type, isNullable, defaultValue, false)
        {
        }

        public IEnumerable<IDatabaseColumn> DependentColumns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ISqlExpression Expression
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsCalculated { get; } = true;
    }
}
