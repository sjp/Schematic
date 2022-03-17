using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Represents a SQLite database. Provides access to SQLite specific behaviour.
/// </summary>
public interface ISqliteDatabase : IRelationalDatabase
{
    /// <summary>
    /// The <c>VACUUM</c> command rebuilds the database file, repacking it into a minimal amount of disk space.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <remarks>This only applies to the main database and not to any attached database.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task VacuumAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The <c>VACUUM</c> command rebuilds the database file, repacking it into a minimal amount of disk space.
    /// </summary>
    /// <param name="schemaName">Either <c>main</c> or the name assigned to an attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// The <c>VACUUM INTO</c> command rebuilds the database file, repacking it into a minimal amount of disk space in a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <remarks>This only applies to the main database and not to any attached database.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task VacuumIntoAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// The <c>VACUUM INTO</c> command rebuilds the database file, repacking it into a minimal amount of disk space in a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <param name="schemaName">Either <c>main</c> or the name assigned to an attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task VacuumIntoAsync(string filePath, string schemaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds another database file to the current database connection.
    /// </summary>
    /// <param name="schemaName">The name to assign for the attached database.</param>
    /// <param name="fileName">The path to a SQLite database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a previously attached database file from the current database connection.
    /// </summary>
    /// <param name="schemaName">The name to assign for the attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default);
}
