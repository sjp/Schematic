using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseViewColumn : IDatabaseColumn
    {
        IRelationalDatabaseView View { get; }
    }
}
