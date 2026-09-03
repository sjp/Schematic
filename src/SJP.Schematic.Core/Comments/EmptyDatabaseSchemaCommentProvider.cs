using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A schema comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseSchemaCommentProvider" />
public sealed class EmptyDatabaseSchemaCommentProvider : IDatabaseSchemaCommentProvider
{
    /// <summary>
    /// Enumerates all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of schema comments.</returns>
    public IAsyncEnumerable<IDatabaseSchemaComments> EnumerateAllSchemaComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSchemaComments>();

    /// <summary>
    /// Retrieves all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of schema comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSchemaComments>> GetAllSchemaComments(CancellationToken cancellationToken = default) => Empty.Tasks.SchemaComments;

    /// <summary>
    /// Retrieves comments for a particular database schema.
    /// </summary>
    /// <param name="schemaName">The name of a database schema.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSchemaComments}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSchemaComments> GetSchemaComments(Identifier schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schemaName);

        return OptionAsync<IDatabaseSchemaComments>.None;
    }
}
