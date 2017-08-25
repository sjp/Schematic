using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable, IRelationalDatabaseViewSync, IRelationalDatabaseViewAsync
    {
        bool IsIndexed { get; }
    }
}
