using System;

namespace SJP.Schematic.Sqlite.Pragma
{
    /// <summary>
    /// Determines optimisation features to use when attempting to optimise the database.
    /// </summary>
    [Flags]
    public enum OptimizeFeatures
    {
        /// <summary>
        /// No flags set, no optimisation will be performed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Debugging mode. Do not actually perform any optimizations but instead return one line of text for each optimization that would have been done. Off by default.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Run <c>ANALYZE</c> on tables that might benefit. On by default.
        /// </summary>
        Analyze = 2,

        /// <summary>
        /// Not yet implemented. Record usage and performance information from the current session in the database file so that it will be available to "optimize" pragmas run by future database connections.
        /// </summary>
        RecordUsage = 4,

        /// <summary>
        /// Not yet implemented. Create indexes that might have been helpful to recent queries.
        /// </summary>
        CreateIndexes = 8
    }
}
