using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyDatabaseSequenceCommentProvider : IDatabaseSequenceCommentProvider
    {
        public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSequenceComments>();

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return OptionAsync<IDatabaseSequenceComments>.None;
        }
    }
}
