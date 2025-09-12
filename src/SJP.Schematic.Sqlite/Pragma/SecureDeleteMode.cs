namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Determines whether secure data deletion is performed on a SQLite database.
/// </summary>
public enum SecureDeleteMode
{
    /// <summary>
    /// No data overwriting is performed. Default behaviour.
    /// </summary>
    Off,

    /// <summary>
    /// Overwrites deleted content with zeros.
    /// </summary>
    On,

    /// <summary>
    /// Will overwrite deleted content with zeros only if doing so does not increase the amount of I/O.
    /// </summary>
    Fast,
}