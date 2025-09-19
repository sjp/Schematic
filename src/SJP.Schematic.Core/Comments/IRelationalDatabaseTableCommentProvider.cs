using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines an object which retrieves comments for database tables.
/// </summary>
/// <seealso cref="IRelationalDatabaseTable"/>
public interface IRelationalDatabaseTableCommentProvider
{
    /// <summary>
    /// Retrieves comments for a particular database table.
    /// </summary>
    /// <param name="tableName">The name of a database table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IRelationalDatabaseTableComments}"/> instance which holds the value of the table's comments, if available.</returns>
    OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments2(CancellationToken cancellationToken = default);
}