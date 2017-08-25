using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase : IRelationalDatabaseSync, IRelationalDatabaseAsync
    {
        IDatabaseDialect Dialect { get; }
    }
}
