using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseViewIndex : IDatabaseIndex<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }
    }
}
