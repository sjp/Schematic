using System;

namespace SJP.Schematic.Oracle
{
    // https://nzdba.wordpress.com/2012/07/24/datapump-vs-application-upgrade/
    [Flags]
    public enum OracleIndexProperties
    {
        None = 0x0000,
        Unique = 0x0001,
        Partitioned = 0x0002,
        Reverse = 0x0004,
        Compressed = 0x0008,
        Functional = 0x0010,
        TempTableIndex = 0x0020,
        SessionSpecificTempTableIndex = 0x0040,
        EmbeddedAdtIndex = 0x0080,
        MaxLengthCheck = 0x0100,
        DomainIndexForIot = 0x0200,
        JoinIndex = 0x0400,
        SystemManagedDomainIndex = 0x0800,
        CreatedByConstraint = 0x1000,
        CreatedByMaterializedView = 0x2000,
        CompositeDomainIndex = 0x8000
    }
}
