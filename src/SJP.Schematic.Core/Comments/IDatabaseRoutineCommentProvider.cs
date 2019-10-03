using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseRoutineCommentProvider
    {
        OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default);
    }
}
