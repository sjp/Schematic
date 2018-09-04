using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{

    public class SqlServerDatabaseViewColumn : SqlServerDatabaseColumn, IDatabaseViewColumn
    {
        public SqlServerDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, IAutoIncrement autoIncrement)
            : base(columnName, type, isNullable, defaultValue, autoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
