using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseViewProvider : IDatabaseViewProvider
    {
        public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseView>();

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return OptionAsync<IDatabaseView>.None;
        }
    }
}
