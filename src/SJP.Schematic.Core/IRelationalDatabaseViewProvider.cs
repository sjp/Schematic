using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseViewProvider
    {
        OptionAsync<IRelationalDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IRelationalDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken));
    }
}
