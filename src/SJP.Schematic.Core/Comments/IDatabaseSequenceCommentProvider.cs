using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseSequenceCommentProvider
    {
        OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default(CancellationToken));
    }
}
