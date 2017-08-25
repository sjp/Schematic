using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseTableColumn : IDatabaseColumn
    {
        IRelationalDatabaseTable Table { get; }
    }
}
