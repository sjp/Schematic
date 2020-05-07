using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Defines an object which retrieves comments for database synonyms.
    /// </summary>
    /// <seealso cref="IDatabaseSynonym"/>
    public interface IDatabaseSynonymCommentProvider
    {
        /// <summary>
        /// Retrieves comments for a particular database synonym.
        /// </summary>
        /// <param name="synonymName">The name of a database synonym.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}"/> instance which holds the value of the synonym's comments, if available.</returns>
        OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all database synonym comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of synonym comments.</returns>
        IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default);
    }
}
