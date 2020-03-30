using System.Collections.Generic;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A rule that will analyse and report potential issues with database synonyms.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface ISynonymRule : IRule
    {
        /// <summary>
        /// Analyses database synonyms.
        /// </summary>
        /// <param name="synonyms">A set of database synonyms.</param>
        /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
        /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
        IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default);
    }
}
