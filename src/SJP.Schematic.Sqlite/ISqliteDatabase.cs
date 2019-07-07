using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    /// <summary>
    /// Represents a SQLite database. Provides access to SQLite specific behaviour.
    /// </summary>
    public interface ISqliteDatabase : IRelationalDatabase
    {
        /// <summary>
        /// <para>The <code>VACUUM</code> command rebuilds the database file, repacking it into a minimal amount of disk space.</para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <remarks>This only applies to the main database and not to any attached database.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VacuumAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The <code>VACUUM</code> command rebuilds the database file, repacking it into a minimal amount of disk space.
        /// </summary>
        /// <param name="schemaName">Either <code>main</code> or the name assigned to an attached database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default);

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
}
