using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyDatabaseRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseRoutineComments>();

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return OptionAsync<IDatabaseRoutineComments>.None;
        }
    }
}
