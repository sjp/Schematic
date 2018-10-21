using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        public MySqlDatabasePrimaryKey(IReadOnlyCollection<IDatabaseColumn> columns)
            : base(Identifier.CreateQualifiedIdentifier("PRIMARY"), DatabaseKeyType.Primary, columns)
        {
        }
    }
}
