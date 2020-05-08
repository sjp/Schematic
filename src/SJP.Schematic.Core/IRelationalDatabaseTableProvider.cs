using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines a relational database table provider that retrieves table information for a database.
    /// </summary>
    public interface IRelationalDatabaseTableProvider
    {
        /// <summary>
        /// Gets a database table.
        /// </summary>
        /// <param name="tableName">A database table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
        OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default);
    }
}
