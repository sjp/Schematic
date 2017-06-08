using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseTableColumn : IDatabaseColumn
    {
        IRelationalDatabaseTable Table { get; }
    }
}
