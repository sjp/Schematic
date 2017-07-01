using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable, IRelationalDatabaseTableSync, IRelationalDatabaseTableAsync
    {
    }
}
