namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Determines where temporary data is stored.
/// </summary>
public enum TemporaryStoreLocation
{
    /// <summary>
    /// The default temporary storage location, as determined by SQLite at compile-time.
    /// </summary>
    Default,

    /// <summary>
    /// Temporary data is stored in files.
    /// </summary>
    File,

    /// <summary>
    /// Temporary data is held in memory.
    /// </summary>
    Memory
}