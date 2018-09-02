using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseViewIndex : SqlServerDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public SqlServerDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseViewColumn> includedColumns, bool isEnabled)
            : base(view, name, isUnique, columns, includedColumns, isEnabled)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
