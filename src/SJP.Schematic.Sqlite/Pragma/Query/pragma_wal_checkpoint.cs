#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information relating to the status of a WAL checkpoint operation.
/// </summary>
public sealed record pragma_wal_checkpoint
{
    /// <summary>
    /// Stores whether the checkpoint operation was successful. Returns <c>0</c> if successful, <c>1</c> if a <c>RESTART</c> or <c>FULL</c> or <c>TRUNCATE</c> checkpoint was blocked from completing.
    /// </summary>
    public required int busy { get; init; }

    /// <summary>
    /// The number of modified pages that have been written to the write-ahead log file.
    /// </summary>
    public required int log { get; init; }

    /// <summary>
    /// The number of pages in the write-ahead log file that have been successfully moved back into the database file at the conclusion of the checkpoint.
    /// </summary>
    public required int checkpointed { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles