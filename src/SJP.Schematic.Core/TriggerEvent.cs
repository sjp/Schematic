using System;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Events that cause a database trigger to fire.
    /// </summary>
    [Flags]
    public enum TriggerEvent
    {
        /// <summary>
        /// Not intended to be used directly. Represents no trigger events available.
        /// </summary>
        None = 0,

        /// <summary>
        /// An <c>INSERT</c> operation on a table.
        /// </summary>
        Insert = 1 << 0,

        /// <summary>
        /// An <c>UPDATE</c> operation on a table.
        /// </summary>
        Update = 1 << 1,

        /// <summary>
        /// An <c>DELETE</c> operation on a table.
        /// </summary>
        Delete = 1 << 2
    }
}
