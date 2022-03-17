namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Determines how file data is synchronised to the file system.
/// </summary>
public enum SynchronousLevel
{
    /// <summary>
    /// The database engine continues without syncing as soon as it has handed data off to the operating system.
    /// </summary>
    Off,

    /// <summary>
    /// The database engine will still sync at the most critical moments, but less often than in <see cref="Full"/> mode.
    /// </summary>
    Normal,

    /// <summary>
    /// The database engine will use the xSync method of the VFS to ensure that all content is safely written to the disk surface prior to continuing.
    /// </summary>
    Full,

    /// <summary>
    /// Like <see cref="Full"/>, with the addition that the directory containing a rollback journal is synced after that journal is unlinked to commit a transaction in <c>DELETE</c> mode.
    /// </summary>
    Extra
}
