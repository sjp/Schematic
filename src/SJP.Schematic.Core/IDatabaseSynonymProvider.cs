using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseSynonymProvider
    {
        Option<IDatabaseSynonym> GetSynonym(Identifier synonymName);

        OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
