using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseViewProvider : IDatabaseViewProvider
    {
        public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken)) => Empty.Views;

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return OptionAsync<IDatabaseView>.None;
        }
    }
}
