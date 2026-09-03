using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A user-defined type comment provider that always returns empty results.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeCommentProvider" />
public sealed class EmptyDatabaseUserDefinedTypeCommentProvider : IDatabaseUserDefinedTypeCommentProvider
{
    /// <summary>
    /// Enumerates all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of user-defined type comments.</returns>
    public IAsyncEnumerable<IDatabaseUserDefinedTypeComments> EnumerateAllUserDefinedTypeComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseUserDefinedTypeComments>();

    /// <summary>
    /// Retrieves all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of user-defined type comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> GetAllUserDefinedTypeComments(CancellationToken cancellationToken = default) => Empty.Tasks.UserDefinedTypeComments;

    /// <summary>
    /// Retrieves comments for a particular database user-defined type.
    /// </summary>
    /// <param name="typeName">The name of a database user-defined type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseUserDefinedTypeComments}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseUserDefinedTypeComments> GetUserDefinedTypeComments(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return OptionAsync<IDatabaseUserDefinedTypeComments>.None;
    }
}
