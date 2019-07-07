using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseSynonymProvider
    {
        OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default);
    }
}
