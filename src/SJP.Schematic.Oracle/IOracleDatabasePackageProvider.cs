using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface IOracleDatabasePackageProvider
    {
        IAsyncEnumerable<IOracleDatabasePackage> GetAllPackages(CancellationToken cancellationToken = default);

        OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default);
    }
}