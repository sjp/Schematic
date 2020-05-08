using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines a database view provider that retrieves view information for a database.
    /// </summary>
    public interface IDatabaseViewProvider
    {
        /// <summary>
        /// Gets a database view.
        /// </summary>
        /// <param name="viewName">A database view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
        OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all database views.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default);
    }
}
