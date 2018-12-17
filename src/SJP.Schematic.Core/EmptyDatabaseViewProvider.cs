using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseViewProvider : IDatabaseViewProvider
    {
        public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken)) => _emptyViews;

        private readonly static Task<IReadOnlyCollection<IDatabaseView>> _emptyViews = Task.FromResult<IReadOnlyCollection<IDatabaseView>>(Array.Empty<IDatabaseView>());

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return OptionAsync<IDatabaseView>.None;
        }
    }
}
