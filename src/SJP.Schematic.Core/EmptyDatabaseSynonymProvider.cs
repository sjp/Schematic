using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A database synonym provider that returns no synonyms. Not intended to be used directly.
    /// </summary>
    /// <seealso cref="IDatabaseSynonymProvider" />
    public sealed class EmptyDatabaseSynonymProvider : IDatabaseSynonymProvider
    {
        /// <summary>Gets all database synonyms. This will always be an empty collection.</summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An empty collection of database synonyms.</returns>
        public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSynonym>();

        /// <summary>
        /// Gets a database synonym. This will always be a 'none' result.
        /// </summary>
        /// <param name="synonymName">A database synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database synonym in the 'none' state.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return OptionAsync<IDatabaseSynonym>.None;
        }
    }
}
