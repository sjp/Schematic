using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseViewColumn : IDatabaseColumn
    {
        IRelationalDatabaseView View { get; }
    }
}
