using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyRelationalDatabaseViewProvider : IRelationalDatabaseViewProvider
    {
        public Task<IReadOnlyCollection<IRelationalDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken)) => _emptyViews;

        private readonly static Task<IReadOnlyCollection<IRelationalDatabaseView>> _emptyViews = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseView>>(Array.Empty<IRelationalDatabaseView>());

        public OptionAsync<IRelationalDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return OptionAsync<IRelationalDatabaseView>.None;
        }
    }
}
