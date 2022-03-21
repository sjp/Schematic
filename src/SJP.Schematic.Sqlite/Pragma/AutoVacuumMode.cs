namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Defines the possible options for the auto vacuum configuration.
/// </summary>
public enum AutoVacuumMode
{
    /// <summary>
    /// Auto vacuum is disabled.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Auto vacuum is performed on every transaction commit.
    /// </summary>
    Full = 1,

    /// <summary>
    /// Auto vacuum is performed only when the <c>incremental_vacuum</c> pragma is invoked.
    /// </summary>
    /// <seealso cref="ISqliteDatabasePragma.IncrementalVacuumAsync(ulong, System.Threading.CancellationToken)"/>
    Incremental = 2
}