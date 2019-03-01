using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyDatabaseRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Empty.RoutineComments;
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return OptionAsync<IDatabaseRoutineComments>.None;
        }
    }
}
