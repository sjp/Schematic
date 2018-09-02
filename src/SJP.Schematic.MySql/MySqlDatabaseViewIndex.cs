using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseViewIndex : MySqlDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public MySqlDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
            : base(view, name, isUnique, columns)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
