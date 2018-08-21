using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        public MySqlDatabasePrimaryKey(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseColumn> columns)
            : base(table, Identifier.CreateQualifiedIdentifier("PRIMARY"), DatabaseKeyType.Primary, columns)
        {
        }
    }
}
