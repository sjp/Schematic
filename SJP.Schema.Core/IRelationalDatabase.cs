using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabase : IRelationalDatabaseSync, IRelationalDatabaseAsync
    {
        IDatabaseDialect Dialect { get; }
    }
}
