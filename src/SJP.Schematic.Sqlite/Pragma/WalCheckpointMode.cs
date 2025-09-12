namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// When WAL checkpointing is enabled, describes the possible checkpointing methods.
/// </summary>
public enum WalCheckpointMode
{
    /// <summary>
    /// Checkpoint as many frames as possible without waiting for any database readers or writers to finish.
    /// </summary>
    Passive,

    /// <summary>
    /// This mode blocks until there is no database writer and all readers are reading from the most recent database snapshot.
    /// </summary>
    Full,

    /// <summary>
    /// This mode works the same way as <see cref="Full"/> with the addition that after checkpointing the log file it blocks until all readers are finished with the log file.
    /// </summary>
    Restart,

    /// <summary>
    /// This mode works the same way as <see cref="Restart"/> with the addition that the WAL file is truncated to zero bytes upon successful completion.
    /// </summary>
    Truncate,
}