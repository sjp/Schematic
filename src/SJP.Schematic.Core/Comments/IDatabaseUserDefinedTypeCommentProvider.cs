using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines an object which retrieves comments for database user-defined types.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedType"/>
public interface IDatabaseUserDefinedTypeCommentProvider
{
    /// <summary>
    /// Retrieves comments for a particular database user-defined type.
    /// </summary>
    /// <param name="typeName">The name of a database user-defined type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseUserDefinedTypeComments}"/> instance which holds the value of the type's comments, if available.</returns>
    OptionAsync<IDatabaseUserDefinedTypeComments> GetUserDefinedTypeComments(Identifier typeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-defined type comments.</returns>
    IAsyncEnumerable<IDatabaseUserDefinedTypeComments> EnumerateAllUserDefinedTypeComments(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-defined type comments.</returns>
    Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> GetAllUserDefinedTypeComments(CancellationToken cancellationToken = default);
}
