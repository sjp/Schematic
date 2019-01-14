using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface IOracleDatabasePackageProvider
    {
        Task<IReadOnlyCollection<IOracleDatabasePackage>> GetAllPackages(CancellationToken cancellationToken = default(CancellationToken));

        OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default(CancellationToken));
    }
}