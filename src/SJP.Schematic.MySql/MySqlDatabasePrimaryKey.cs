using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        public MySqlDatabasePrimaryKey(IRelationalDatabaseTable table, IEnumerable<IDatabaseColumn> columns)
            : base(table, new Identifier("PRIMARY"), DatabaseKeyType.Primary, columns)
        {
        }
    }
}
