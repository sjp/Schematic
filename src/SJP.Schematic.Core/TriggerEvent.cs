using System;

namespace SJP.Schematic.Core;

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
    Delete = 1 << 2,

    /// <summary>
    /// A <c>TRUNCATE</c> operation on a table. Only PostgreSQL supports triggering on this event.
    /// </summary>
    Truncate = 1 << 3,

    /// <summary>
    /// An event that the database recognises but Schematic does not model. The trigger is still
    /// returned, and its <see cref="IDatabaseTrigger.Definition"/> describes what it fires on.
    /// </summary>
    Other = 1 << 30,
}
