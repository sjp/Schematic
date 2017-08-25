using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseTableIndex : IDatabaseIndex<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }
    }
}
