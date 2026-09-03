using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines an object which retrieves comments for database schemas.
/// </summary>
/// <seealso cref="IDatabaseSchema"/>
public interface IDatabaseSchemaCommentProvider
{
    /// <summary>
    /// Retrieves comments for a particular database schema.
    /// </summary>
    /// <param name="schemaName">The name of a database schema.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSchemaComments}"/> instance which holds the value of the schema's comments, if available.</returns>
    OptionAsync<IDatabaseSchemaComments> GetSchemaComments(Identifier schemaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of schema comments.</returns>
    IAsyncEnumerable<IDatabaseSchemaComments> EnumerateAllSchemaComments(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of schema comments.</returns>
    Task<IReadOnlyCollection<IDatabaseSchemaComments>> GetAllSchemaComments(CancellationToken cancellationToken = default);
}
