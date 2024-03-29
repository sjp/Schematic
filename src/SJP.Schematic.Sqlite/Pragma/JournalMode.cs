﻿namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Defines the possible journaling modes available to a SQLite database.
/// </summary>
public enum JournalMode
{
    /// <summary>
    /// Default behaviour. The rollback journal is deleted at the conclusion of each transaction.
    /// </summary>
    Delete,

    /// <summary>
    /// The <c>TRUNCATE</c> journaling mode commits transactions by truncating the rollback journal to zero-length instead of deleting it.
    /// </summary>
    Truncate,

    /// <summary>
    /// The <c>PERSIST</c> journaling mode prevents the rollback journal from being deleted at the end of each transaction.
    /// </summary>
    Persist,

    /// <summary>
    /// The <c>MEMORY</c> journaling mode stores the rollback journal in volatile RAM.
    /// </summary>
    Memory,

    /// <summary>
    /// The <c>WAL</c> journaling mode uses a write-ahead log instead of a rollback journal to implement transactions.
    /// </summary>
    Wal,

    /// <summary>
    /// The <c>WAL2</c> journaling mode uses two write-ahead logs instead of a rollback journal to implement transactions.
    /// It avoids blocking writers during checkpointing by switching write operations to a second write-ahead log when a checkpoint occurs.
    /// See <see href="https://sqlite.org/cgi/src/doc/wal2/doc/wal2.md"/>
    /// </summary>
    Wal2,

    /// <summary>
    /// The <c>OFF</c> journaling mode disables the rollback journal completely.
    /// </summary>
    Off
}