using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines an object which retrieves comments for database views.
/// </summary>
/// <seealso cref="IDatabaseView"/>
public interface IDatabaseViewCommentProvider
{
    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}"/> instance which holds the value of the view's comments, if available.</returns>
    OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default);
}