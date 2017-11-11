using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Sqlite.Pragma
{
    public enum JournalMode
    {
        Delete,
        Truncate,
        Persist,
        Memory,
        Wal,
        Off
    }
}
