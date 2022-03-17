namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Determines how a SQLite database performs locking for a given database.
/// </summary>
public enum LockingMode
{
    /// <summary>
    /// In <c>NORMAL</c> locking-mode (the default), a database connection unlocks the database file at the conclusion of each read or write transaction.
    /// </summary>
    Normal,

    /// <summary>
    /// In <c>EXCLUSIVE</c> locking-mode, the database connection never releases file-locks.
    /// </summary>
    Exclusive
}
