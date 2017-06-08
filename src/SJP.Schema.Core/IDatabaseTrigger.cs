using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IDatabaseTrigger : IDatabaseEntity
    {
        IRelationalDatabaseTable Table { get; }

        string Definition { get; }

        TriggerQueryTiming QueryTiming { get; }

        TriggerEvent TriggerEvent { get; }
    }
}
