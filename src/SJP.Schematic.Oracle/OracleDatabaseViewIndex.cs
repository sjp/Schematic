using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseViewIndex : OracleDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public OracleDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, OracleIndexProperties properties)
            : base(view, name, isUnique, columns, properties)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
