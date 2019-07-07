using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface ISynonymRule : IRule
    {
        IEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms);

        Task<IEnumerable<IRuleMessage>> AnalyseSynonymsAsync(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default);
    }
}
