using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    [Flags]
    public enum TriggerEvent
    {
        None = 0,
        Insert = 1,
        Update = 2,
        Delete = 4,
        Truncate = 8
    }
}
