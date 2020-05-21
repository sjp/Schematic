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
        None                            = 0,

        /// <summary>
        /// Whether the index is unique.
        /// </summary>
        Unique                          = 1 << 0,

        /// <summary>
        /// When set, the index is partitioned.
        /// </summary>
        Partitioned                     = 1 << 1,

        /// <summary>
        /// Reverse
        /// </summary>
        Reverse                         = 1 << 2,

        /// <summary>
        /// When set, the index is compressed.
        /// </summary>
        Compressed                      = 1 << 3,

        /// <summary>
        /// The index is a function-based index.
        /// </summary>
        Functional                      = 1 << 4,

        /// <summary>
        /// The index is an index on a temporary table.
        /// </summary>
        TempTableIndex                  = 1 << 5,

        /// <summary>
        /// The index is a session-specific index on a temporary table.
        /// </summary>
        SessionSpecificTempTableIndex   = 1 << 6,

        /// <summary>
        /// EmbeddedAdtIndex
        /// </summary>
        EmbeddedAdtIndex                = 1 << 7,

        /// <summary>
        /// MaxLengthCheck
        /// </summary>
        MaxLengthCheck                  = 1 << 8,

        /// <summary>
        /// DomainIndexForIot
        /// </summary>
        DomainIndexForIot               = 1 << 9,

        /// <summary>
        /// JoinIndex
        /// </summary>
        JoinIndex                       = 1 << 10,

        /// <summary>
        /// SystemManagedDomainIndex
        /// </summary>
        SystemManagedDomainIndex        = 1 << 11,

        /// <summary>
        /// When set, the index was created by a constraint, and not explicitly.
        /// </summary>
        CreatedByConstraint             = 1 << 12,

        /// <summary>
        /// When set, the index was created by a materialized view, and not explicitly.
        /// </summary>
        CreatedByMaterializedView       = 1 << 13,

        /// <summary>
        /// Unknown -- to be determined what this actually does.
        /// </summary>
        Unknown                         = 1 << 14,

        /// <summary>
        /// CompositeDomainIndex
        /// </summary>
        CompositeDomainIndex            = 1 << 15
    }
}
