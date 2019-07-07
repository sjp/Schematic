using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseSynonymCommentProvider
    {
        OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default);
    }
}
