using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Sqlite.Pragma
{
    public enum WalCheckpointMode
    {
        Passive,
        Full,
        Restart,
        Truncate
    }
}
