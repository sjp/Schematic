using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// Defines a database package provider that retrieves package information for a database.
    /// </summary>
    public interface IOracleDatabasePackageProvider
    {
        /// <summary>
        /// Gets all database packages.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database packages.</returns>
        IAsyncEnumerable<IOracleDatabasePackage> GetAllPackages(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a database package.
        /// </summary>
        /// <param name="packageName">A database package name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database package in the 'some' state if found; otherwise 'none'.</returns>
        OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default);
    }
}