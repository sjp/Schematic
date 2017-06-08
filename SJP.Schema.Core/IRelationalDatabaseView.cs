using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable, IRelationalDatabaseViewSync, IRelationalDatabaseViewAsync
    {
        bool IsIndexed { get; }
    }
}
