using System;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A set of properties that apply to an Oracle database index.
    /// </summary>
    // https://nzdba.wordpress.com/2012/07/24/datapump-vs-application-upgrade/
    [Flags]
    public enum OracleIndexProperties
    {
        /// <summary>
        /// No properties defined.
        /// </summary>
        None                            = 0x0000,

        /// <summary>
        /// Whether the index is unique.
        /// </summary>
        Unique                          = 0x0001,

        /// <summary>
        /// When set, the index is partitioned.
        /// </summary>
        Partitioned                     = 0x0002,

        /// <summary>
        /// Reverse
        /// </summary>
        Reverse                         = 0x0004,

        /// <summary>
        /// When set, the index is compressed.
        /// </summary>
        Compressed                      = 0x0008,

        /// <summary>
        /// The index is a function-based index.
        /// </summary>
        Functional                      = 0x0010,

        /// <summary>
        /// The index is an index on a temporary table.
        /// </summary>
        TempTableIndex                  = 0x0020,

        /// <summary>
        /// The index is a session-specific index on a temporary table.
        /// </summary>
        SessionSpecificTempTableIndex   = 0x0040,

        /// <summary>
        /// EmbeddedAdtIndex
        /// </summary>
        EmbeddedAdtIndex                = 0x0080,

        /// <summary>
        /// MaxLengthCheck
        /// </summary>
        MaxLengthCheck                  = 0x0100,

        /// <summary>
        /// DomainIndexForIot
        /// </summary>
        DomainIndexForIot               = 0x0200,

        /// <summary>
        /// JoinIndex
        /// </summary>
        JoinIndex                       = 0x0400,

        /// <summary>
        /// SystemManagedDomainIndex
        /// </summary>
        SystemManagedDomainIndex        = 0x0800,

        /// <summary>
        /// When set, the index was created by a constraint, and not explicitly.
        /// </summary>
        CreatedByConstraint             = 0x1000,

        /// <summary>
        /// When set, the index was created by a materialized view, and not explicitly.
        /// </summary>
        CreatedByMaterializedView       = 0x2000,

        /// <summary>
        /// Unknown -- to be determined what this actually does.
        /// </summary>
        Unknown                         = 0x4000,

        /// <summary>
        /// CompositeDomainIndex
        /// </summary>
        CompositeDomainIndex            = 0x8000
    }
}
