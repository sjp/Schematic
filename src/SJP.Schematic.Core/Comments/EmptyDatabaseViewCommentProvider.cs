using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyDatabaseViewCommentProvider : IDatabaseViewCommentProvider
    {
        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return OptionAsync<IDatabaseViewComments>.None;
        }

        public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseViewComments>();
    }
}
