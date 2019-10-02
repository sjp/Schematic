using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseSequenceCommentProvider
    {
        OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default);
    }
}
