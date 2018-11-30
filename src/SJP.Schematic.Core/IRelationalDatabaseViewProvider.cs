using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseViewProvider
    {
        Option<IRelationalDatabaseView> GetView(Identifier viewName);

        OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        IReadOnlyCollection<IRelationalDatabaseView> Views { get; }

        Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
