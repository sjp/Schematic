using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseViewProvider
    {
        OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default);
    }
}
